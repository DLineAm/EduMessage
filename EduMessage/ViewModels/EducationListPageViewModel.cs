using EduMessage.Services;

using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Toolkit.Uwp.Helpers;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationListPageViewModel
    {
        [Property] private ObservableCollection<CourseFiles> _courses = new();
        [Property] private ObservableCollection<EducationFile> _files = new();
        [Property] private bool _isClearButtonEnabled;
        [Property] private string _courseTitle;
        [Property] private string _courseDescription;
        private Speciality _speciality;

        public async Task Initialize(Speciality speciality)
        {
            App.DropCompleted += App_DropCompleted;
            _speciality = speciality;

            try
            {
                var response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                    .GetStringAsync(App.Account.Jwt))
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
                    //var attachments = sortedList.Select(async f => f.IdAttachmanentNavigation != null 
                    //? new EducationFile
                    //{
                    //    Name = f.IdAttachmanentNavigation.Title,
                    //    Data = f.IdAttachmanentNavigation.Data,
                    //    Type = "." + f.IdAttachmanentNavigation.Title.Split('.').LastOrDefault(),
                    //    ImagePath = await GetImage("." + f.IdAttachmanentNavigation.Title.Split('.').LastOrDefault(), f.IdAttachmanentNavigation.Data)
                    //}
                    //: null)
                    //    .Select(t=> t.Result).ToList();

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

            }
            catch (Exception e)
            {

            }
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

            var attachments = coursesList.Select(c => c.IdAttachmanentNavigation).ToList();
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
                            .PostBoolAsync(courseAttachment, App.Account.Jwt))
                        .DeserializeJson<int>();
                }
                else
                {
                    response = (await (App.Address + "Education/Courses.FromList")
                            .PostBoolAsync(coursesList, App.Account.Jwt))
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
                    Cancel();
                }
            }
            catch (Exception e)
            {


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
                    if (fileProps.Size == 0)
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
                }
            }

            return result;
        }


        private async Task<object> GetImage(string type, byte[] data) => type switch
        {
            ".docx" => "ms-appx:///Assets/word.png",
            ".pdf" => "ms-appx:///Assets/pdf.png",
            ".txt" => "ms-appx:///Assets/txt.png",
            ".png" or ".jpg" or ".jpeg" => await CreateBitmap(data),
            _ => "ms-appx:///Assets/file.png"
        };

        private async Task<BitmapImage> CreateBitmap(byte[] data)
        {
            var source = new BitmapImage();
            using var stream = new InMemoryRandomAccessStream();
            await stream.WriteAsync(data.AsBuffer());
            stream.Seek(0);
            await source.SetSourceAsync(stream);

            return source;
        }

        [Command]
        private async void DeleteCourse(object courseObj)
        {
            if (courseObj is CourseFiles course)
            {
                try
                {
                    var response = (await (App.Address + $"Education/Courses.id={course.Course.Id}")
                            .DeleteFromRequestAsync(App.Account.Jwt))
                        .DeserializeJson<bool>();

                    if (response)
                    {
                        Courses.Remove(course);
                    }
                }
                catch (Exception e)
                {

                }

            }
        }
    }

    public class EducationFile
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public object ImagePath { get; set; }

        public byte[] Data { get; set; }
    }

    public class CourseFiles
    {
        public Course Course { get; set; }
        public List<EducationFile> Files { get; set; }

        public Visibility FilesInfoVisibility => Files == null
        || Files.Count == 0
        || Files.Count == 1 && Files[0] == null
        ? Visibility.Collapsed
        : Visibility.Visible;
    }
}
