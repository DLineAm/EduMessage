﻿using EduMessage.Services;

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
        [Property] private string _dialogTitle;
        [Property] private string _courseDescription;
        [Property] private Visibility _noResultsFoundAnimationVisibility = Visibility.Collapsed;
        [Property] private Visibility _teacherInputVisibility;
        [Property] private GridLength _gridWidth;

        private bool _isCourseAddMode;
        private CourseFiles _selectedCourse;
        private Speciality _speciality;

        public async Task Initialize(Speciality speciality)
        {
            App.DropCompleted += App_DropCompleted;
            _speciality = speciality;
            var role = App.Account.User.IdRole;

            UpdateTeacherInputVisibility(role == 2 ? Visibility.Visible : Visibility.Collapsed);

            try
            {
                var response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                    .SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                    .DeserializeJson<List<CourseAttachment>>();

                Courses = new ObservableCollection<CourseFiles>(await ConvertToCourseFiles(response));

                if (Courses.Count == 0)
                {
                    UpdateNoResultsFoundVisibility(Visibility.Visible);
                }

            }
            catch (Exception e)
            {

            }
        }

        private async Task<List<CourseFiles>> ConvertToCourseFiles(List<CourseAttachment> courseAttachments)
        {
            var savedCourseId = -1;
            List<CourseFiles> result = new();

            foreach (var courseAttachmanent in courseAttachments)
            {
                var course = courseAttachmanent.IdCourseNavigation;
                var convertedAttachments = new List<EducationFile>();
                if (savedCourseId != -1
                    && savedCourseId == course.Id)
                {
                    continue;
                }

                var sortedList = courseAttachments.Where(c => c.IdCourse == course.Id).ToList();
                var attachments = sortedList.Select(f => f.IdAttachmanentNavigation).ToList();

                foreach (var attachment in attachments)
                {

                    if (attachment == null)
                    {
                        continue;
                    }

                    var courseId = sortedList.FirstOrDefault(a => a.IdAttachmanentNavigation.Id == attachment.Id);

                    convertedAttachments.Add(new EducationFile
                    {
                        Id = courseId.Id,
                        AttachmentId = attachment.Id,
                        Name = attachment.Title,
                        Type = "." + attachment.Title.Split('.').Last(),
                        Data = attachment.Data,
                        ImagePath = await GetImage("." + attachment.Title.Split('.').Last(), attachment.Data)
                    });
                }

                var filesPair = convertedAttachments.Select(f => new KeyValuePair<EducationFile, int>(f, f.Id));

                var convertedCourse = new CourseFiles
                {
                    CoursesId = sortedList.Select(c => c.Id).ToList(),
                    Course = course,
                    Files = convertedAttachments,
                    TestFiles = filesPair
                };

                result.Add(convertedCourse);

                savedCourseId = course.Id;
            }

            return result;
        }

        private void UpdateTeacherInputVisibility(Visibility visibility)
        {
            TeacherInputVisibility = visibility;
            GridWidth = visibility == Visibility.Visible
                ? new GridLength(80, GridUnitType.Pixel)
                : new GridLength(0, GridUnitType.Pixel);
        }


        private void UpdateNoResultsFoundVisibility(Visibility visibility)
        {
            NoResultsFoundAnimationVisibility = visibility;
        }

        [Command]
        private void CleanFilesList()
        {
            Files.Clear();
            UpdateAccessibility();
        }

        [Command]
        private void InitializeChangeCourseDialog(object courseObj)
        {
            if (courseObj is CourseFiles course)
            {
                _isCourseAddMode = false;

                DialogTitle = "Изменение курса";
                var educationCourse = course.Course;

                _selectedCourse = course;

                Files.Clear();
                course.Files.ForEach(Files.Add);

                CourseTitle = educationCourse.Title;
                CourseDescription = educationCourse.Description;

                EventAggregator.Publish(new CourseDialogStartShowing(_isCourseAddMode));
            }
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

            Course course = new Course
            {
                Id = _selectedCourse?.Course?.Id ?? 0,
                Title = CourseTitle,
                Description = CourseDescription,
                IdSpeciality = _speciality.Id,
                IdSpecialityNavigation = _speciality
            };

            //if (_selectedCourse != null)
            //{
            //    course = _selectedCourse.Course;
            //    course.Title = CourseTitle;
            //    course.Description = CourseDescription;
            //}
            //else
            //{
            //    course = new Course
            //    {
            //        Title = CourseTitle,
            //        Description = CourseDescription,
            //        IdSpecialityNavigation = _speciality,
            //    };
            //}     

            List<CourseAttachment> coursesList = Files.Select(s => new CourseAttachment
            {
                IdAttachmanentNavigation = new Attachment
                {
                    IdType = ConvertFileType(s.Type),
                    Data = s.Data,
                    Title = s.Name
                },

                Id = s.Id,
                IdAttachmanent = s.AttachmentId,
                IdCourseNavigation = course,
                IdCourse = course.Id
            }).ToList();

            List<Attachment> attachments = coursesList.Select(c => c.IdAttachmanentNavigation).ToList();
            try
            {
                var response = new KeyValuePair<int, List<int>>();
                if (_selectedCourse != null)
                {
                    if (coursesList.Count == 0)
                    {
                        coursesList.Add(new CourseAttachment
                        {
                            Id = _selectedCourse.CoursesId.FirstOrDefault(),
                            IdCourse = course.Id,
                            IdCourseNavigation = course
                        });
                    }

                    var putResponse = (await (App.Address + "Education/Courses")
                        .SendRequestAsync(coursesList, HttpRequestType.Put, App.Account.Jwt))
                        .DeserializeJson<bool>();

                    if (putResponse)
                    {
                        _selectedCourse.Course.Title = CourseTitle;
                        _selectedCourse.Course.Description = CourseDescription;
                        _selectedCourse.Files = Files.ToList();
                    }

                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, ""));
                    _selectedCourse = null;

                    return;
                }
                if (coursesList.Count == 0)
                {
                    coursesList.Add( new CourseAttachment
                    {
                        IdCourseNavigation = course
                    });
                    //var courseAttachment = new CourseAttachment
                    //{
                    //    IdCourseNavigation = course
                    //};
                    //var pairResponse = (await (App.Address + "Education/Courses")
                    //        .SendRequestAsync(courseAttachment, HttpRequestType.Post, App.Account.Jwt))
                    //    .DeserializeJson<KeyValuePair<int, int>>();

                    //response = new KeyValuePair<int, List<int>>(pairResponse.Key, new List<int>{pairResponse.Value});
                }

                    response = (await (App.Address + "Education/Courses.FromList")
                            .SendRequestAsync(coursesList, HttpRequestType.Post, App.Account.Jwt))
                        .DeserializeJson<KeyValuePair<int, List<int>>>();

                if (response.Key != -1)
                {
                    var responseKey = response.Key;
                    var convertedAttachments = new List<EducationFile>();
                    course.Id = responseKey;
                    coursesList.ForEach(c => c.IdCourseNavigation.Id = responseKey);
                    foreach (var attachment in attachments)
                    {
                        if (attachment == null)
                        {
                            continue;
                        }

                        var typeString = "." + attachment.Title.Split('.').Last();

                        var attachmentIndex = attachments.IndexOf(attachment);

                        var findedId = response.Value[attachmentIndex];

                        convertedAttachments.Add(new EducationFile
                        {
                            Id = findedId,
                            Name = attachment.Title,
                            Type = typeString,
                            Data = attachment.Data,
                            ImagePath = await GetImage(typeString, attachment.Data)
                        });
                    }
                    var convertedCourse = new CourseFiles
                    {
                        CoursesId = response.Value,
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
        private void InitializeAddCourseDialog()
        {
            _selectedCourse = null;
            _isCourseAddMode = true;

            Files.Clear();
            CourseTitle = string.Empty;
            CourseDescription = string.Empty;

            EventAggregator.Publish(new CourseDialogStartShowing(_isCourseAddMode));
        }

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
    public record CourseDialogStartShowing(bool isAddMode);
}
