using EduMessage.Services;

using Microsoft.Toolkit.Uwp.Helpers;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class EducationListPageViewModel
    {
        [Property] private ObservableCollection<CourseFiles> _courses = new();
        [Property] private ObservableCollection<EducationFile> _files = new();
        [Property] private bool _isClearButtonEnabled;
        [Property] private string _courseTitle;
        [Property] private string _courseDescription;
        [Property] private Visibility _noResultsFoundAnimationVisibility = Visibility.Collapsed;
        [Property] private Visibility _teacherInputVisibility;
        [Property] private GridLength _gridWidth;

        private Speciality _speciality;

        public async Task Initialize(Speciality speciality)
        {
            App.DropCompleted += App_DropCompleted;
            _speciality = speciality;
            var role = App.Account.User.IdRole;
            TeacherInputVisibility = role == 2 ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                var response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                    .SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                    .DeserializeJson<List<CourseAttachment>>();

                var courses = response
                    .Select(c => c.IdCourseNavigation)
                    .Distinct()
                    .ToList();

                var savedCourseId = -1;

                foreach (var course in courses)
                {
                    var convertedAttachments = new List<EducationFile>();
                    if (savedCourseId != -1
                        && savedCourseId == course.Id)
                    {
                        continue;
                    }

                    var sortedList = response.Where(c => c.IdCourse == course.Id).ToList();
                    var attachments = sortedList.Select(f => f.IdAttachmanentNavigation).ToList();

                    foreach (var attachment in attachments)
                    {
                        if (attachment == null)
                        {
                            break;
                        }

                        convertedAttachments.Add(new EducationFile
                        {

                            Name = attachment.Title,
                            Type = "." + attachment.Title.Split('.').Last(),
                            Data = attachment.Data,
                            ImagePath = await GetImage("." + attachment.Title.Split('.').Last(), attachment.Data)
                        });
                    }

                    var convertedCourse = new CourseFiles
                    {
                        Course = course,
                        Files = convertedAttachments
                    };

                    Courses.Add(convertedCourse);

                    savedCourseId = course.Id;
                }

                if (Courses.Count == 0)
                {
                    UpdateNoResultsFoundVisibility(Visibility.Visible);
                }

            }
            catch (Exception e)
            {

            }
        }

        private void UpdateNoResultsFoundVisibility(Visibility visibility)
        {
            NoResultsFoundAnimationVisibility = visibility;
            GridWidth = visibility == Visibility.Visible 
                ? new GridLength( 80, GridUnitType.Pixel) 
                : new GridLength(0, GridUnitType.Pixel);
        }

        [Command]
        private void CleanFilesList()
        {
            Files.Clear();
            UpdateAccessibility();
        }

        [Command]
        private async void Apply()
        {
            if (string.IsNullOrWhiteSpace(CourseTitle)
                || string.IsNullOrWhiteSpace(CourseDescription))
            {
                return;
            }

            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Сохранение курса"));

            var course = new Course
            {
                Title = CourseTitle,
                Description = CourseDescription,
                IdSpecialityNavigation = _speciality
            };

            List<CourseAttachment> coursesList = Files.Select(s => new CourseAttachment
            {
                IdAttachmanentNavigation = new Attachment
                {
                    IdType = ConvertFileType(s.Type),
                    Data = s.Data,
                    Title = s.Name
                },
                IdCourseNavigation = course
            }).ToList();

            List<Attachment> attachments = coursesList.Select(c => c.IdAttachmanentNavigation).ToList();
            try
            {
                int response;
                if (coursesList.Count == 0)
                {
                    var courseAttachment = new CourseAttachment
                    {
                        IdCourseNavigation = course
                    };
                    response = (await (App.Address + "Education/Courses")
                            .SendRequestAsync(courseAttachment, HttpRequestType.Post, App.Account.Jwt))
                        .DeserializeJson<int>();
                }
                else
                {
                    response = (await (App.Address + "Education/Courses.FromList")
                            .SendRequestAsync(coursesList, HttpRequestType.Post, App.Account.Jwt))
                        .DeserializeJson<int>();
                }

                if (response != -1)
                {
                    var convertedAttachments = new List<EducationFile>();
                    course.Id = response;
                    coursesList.ForEach(c => c.IdCourseNavigation.Id = response);
                    foreach (var attachment in attachments)
                    {
                        if (attachment == null)
                        {
                            continue;
                        }

                        var typeString = "." + attachment.Title.Split('.').Last();

                        convertedAttachments.Add(new EducationFile
                        {

                            Name = attachment.Title,
                            Type = typeString,
                            Data = attachment.Data,
                            ImagePath = await GetImage(typeString, attachment.Data)
                        });
                    }
                    var convertedCourse = new CourseFiles
                    {
                        Course = course,
                        Files = convertedAttachments
                    };
                    Courses.Add(convertedCourse);
                    Cancel();
                }

                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                UpdateNoResultsFoundVisibility(Visibility.Collapsed);
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }

        [Command]
        private async void OpenFile(object fileObj)
        {
            if (fileObj is EducationFile file)
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile storageFile;
                if (await storageFolder.FileExistsAsync(file.Name))
                {
                    storageFile = await storageFolder.GetFileAsync(file.Name);
                    await FileIO.WriteTextAsync(storageFile, "");
                }
                else
                {
                    storageFile = await storageFolder.CreateFileAsync(file.Name);
                }

                await FileIO.WriteBytesAsync(storageFile, file.Data);
                await Launcher.LaunchFileAsync(storageFile);
            }
        }

        private int ConvertFileType(string type) => type switch
        {
            ".pdf" => 1,
            ".txt" => 2,
            ".png" or ".jpg" or ".jpeg" => 3,
            ".docx" or ".doc" => 4,
            _ => 5
        };

        [Command]
        private void Cancel()
        {
            CourseTitle = string.Empty;
            CourseDescription = string.Empty;
            Files.Clear();
        }


        [Command]
        private void DeleteFile(object file)
        {
            var convertedFile = file as EducationFile;
            Files.Remove(convertedFile);
            UpdateAccessibility();
        }

        private void UpdateAccessibility()
        {
            IsClearButtonEnabled = Files.Count > 0;
        }

        private async void App_DropCompleted(IReadOnlyList<Windows.Storage.IStorageItem> items)
        {
            try
            {
                var files = await ReadFiles(items);

                foreach (var file in files)
                {
                    Files.Add(file);
                }

                UpdateAccessibility();
            }
            catch (Exception e)
            {

            }
        }

        private async Task<List<EducationFile>> ReadFiles(IReadOnlyList<Windows.Storage.IStorageItem> items)
        {
            var result = new List<EducationFile>();
            foreach (var item in items)
            {
                if (item is StorageFolder folder)
                {
                    var filesInFolder = await ReadFiles(await folder.GetFilesAsync());
                    var filesInFolders = await ReadFiles(await folder.GetFoldersAsync());

                    result.AddRange(filesInFolder);
                    result.AddRange(filesInFolders);
                }
                else if (item is StorageFile file)
                {
                    var fileProps = await file.GetBasicPropertiesAsync();
                    if (fileProps.Size == 0 && fileProps.Size >= 250 * 1024 * 1024)
                    {
                        continue;
                    }
                    var buffer = await FileIO.ReadBufferAsync(file);
                    var data = buffer.ToArray();
                    await Task.Delay(TimeSpan.FromMilliseconds(1));

                    result.Add(new EducationFile
                    {
                        Name = file.Name,
                        Type = file.FileType,
                        Data = data,
                        ImagePath = await GetImage(file.FileType, data)
                    });

                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }
            }

            return result;
        }


        private async Task<object> GetImage(string type, byte[] data) => type switch
        {
            ".docx" => "ms-appx:///Assets/word.png",
            ".pdf" => "ms-appx:///Assets/pdf.png",
            ".txt" => "ms-appx:///Assets/txt.png",
            ".png" or ".jpg" or ".jpeg" => await data.CreateBitmap(),
            _ => "ms-appx:///Assets/file.png"
        };

        [Command]
        private async void DeleteCourse(object courseObj)
        {
            if (courseObj is CourseFiles course)
            {
                try
                {
                    var response = (await (App.Address + $"Education/Courses.id={course.Course.Id}")
                            .SendRequestAsync("", HttpRequestType.Delete, App.Account.Jwt))
                        .DeserializeJson<bool>();

                    if (response)
                    {
                        Courses.Remove(course);
                    }

                    if (Courses.Count == 0)
                    {
                        UpdateNoResultsFoundVisibility(Visibility.Visible);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }
    }

    public record LoaderVisibilityChanged(Visibility LoaderVisibility, string LoaderText);
}
