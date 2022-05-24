﻿using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class EducationListPageViewModel : IEventSubscriber<DropCompletedEvent>
    {
        [Property] private ObservableCollection<FormattedCourse> _courses = new();
        [Property] private ObservableCollection<Attachment> _files = new();
        [Property] private bool _isClearButtonEnabled;
        [Property] private string _courseTitle;
        [Property] private string _courseDescription;
        [Property] private Visibility _noResultsFoundAnimationVisibility = Visibility.Collapsed;
        [Property] private Visibility _teacherInputVisibility;
        [Property] private GridLength _gridWidth;
        [Property] private string _errorText;

        private bool _isCourseAddMode;
        private FormattedCourse _selectedCourse;
        private Speciality _speciality;

        public async Task Initialize(Speciality speciality)
        {
            _speciality = speciality;

            UpdateTeacherInputVisibility(App.Account.GetUser().IdRole == 2 ? Visibility.Visible : Visibility.Collapsed);

            try
            {
                var response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                    .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<CourseAttachment>>();

                Courses = new ObservableCollection<FormattedCourse>(await ConvertToFormattedCourse(response));

                if (Courses.Count == 0)
                {
                    UpdateNoResultsFoundVisibility(Visibility.Visible);
                }

            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {
            }
        }

        private async Task<List<FormattedCourse>> ConvertToFormattedCourse(List<CourseAttachment> response)
        {
            var formattedCourses = new List<FormattedCourse>();
            foreach (var courseAttachment in response)
            {
                var course = courseAttachment.IdCourseNavigation;
                var any = false;
                foreach (var c in formattedCourses)
                {
                    if (c.Course.Id == course.Id)
                    {
                        any = true;
                        break;
                    }
                }

                if (any)
                {
                    continue;
                }

                var attachments = response.Where(c => c.IdCourse == course.Id && c.IdAttachmanentNavigation != null)
                    .Select(c => c.IdAttachmanentNavigation).ToList();

                attachments.ForEach(async a => await a.SplitAndGetImage());

                var formattedCourse = new FormattedCourse
                {
                    Id = courseAttachment.Id,
                    Course = course,
                    Attachments = new ObservableCollection<Attachment>( attachments)
                };

                if (!attachments.Any())
                {
                    formattedCourse.Attachments = null;
                }

                formattedCourses.Add(formattedCourse);

                
            }

            return formattedCourses;
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
            if (courseObj is not FormattedCourse course) return;

            _isCourseAddMode = false;

            var educationCourse = course.Course;

            _selectedCourse = course;

            Files.Clear();
            if (course.Attachments != null)
            {
                foreach (var courseAttachment in course.Attachments)
                {
                    Files.Add(courseAttachment);
                } 
            }

            CourseTitle = educationCourse.Title;
            CourseDescription = educationCourse.Description;

            EventAggregator.Publish(new CourseDialogStartShowing(_isCourseAddMode));
        }

        [Command]
        private async void Apply()
        {
            ErrorText = "";
            if (string.IsNullOrWhiteSpace(CourseTitle)
                || string.IsNullOrWhiteSpace(CourseDescription))
            {
                ErrorText = "Все поля должы быть заполнены!";
                EventAggregator.Publish(new CourseAddedOrChangedEvent(false));
                return;
            }

            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Сохранение курса"));

            var selectedCourse = _selectedCourse?.Course;
            Course course = new Course
                {
                    Id = selectedCourse?.Id ?? 0,
                    Title = CourseTitle,
                    Description = CourseDescription,
                    IdSpeciality = _speciality.Id
                };

            List<Attachment> attachments = Files.ToList();

            var courses = new List<CourseAttachment>();

            bool isZeroAttachments = attachments.Count == 0;

            foreach (var attachment in attachments)
            {
                var courseToAdd = new CourseAttachment
                {
                    IdCourse = course.Id,
                    IdCourseNavigation = course,
                    IdAttachmanentNavigation = attachment
                };
                if (attachment != null && attachment.Id != 0)
                {
                    courseToAdd.IdAttachmanent = attachment.Id;
                }

                courses.Add(courseToAdd);
            }

            if (isZeroAttachments)
            {
                var courseToAdd = new CourseAttachment
                {
                    Id = _selectedCourse?.Id ?? 0,
                    IdCourseNavigation = course
                };
                if (course.Id != 0)
                {
                    courseToAdd.IdCourse = course.Id;
                }
                courses.Add(courseToAdd);
            }

            try
            {
                if (!_isCourseAddMode)
                {
                    await SendCoursePutRequest(courses);
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Курс изменен!"));
                    EventAggregator.Publish(new CourseAddedOrChangedEvent(true));
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                    UpdateNoResultsFoundVisibility(Visibility.Collapsed);
                    return;
                }

                var (courseId, attachmentIds) = (await (App.Address + "Education/Courses.FromList")
                        .SendRequestAsync(courses, HttpRequestType.Post, App.Account.GetJwt()))
                    .DeserializeJson<KeyValuePair<int, List<int>>>();

                if (courseId != -1)
                {
                    course.Id = courseId;
                    for (int i = 0; i < attachmentIds.Count; i++)
                    {
                        var attachmentId = attachmentIds[i];
                        var courseAttachment = courses[i];
                        courseAttachment.Id = attachmentId;
                        courseAttachment.IdCourse = courseId;
                    }
                    List<FormattedCourse> formattedCourses = await ConvertToFormattedCourse(courses);
#pragma warning disable HAA0603 // Delegate allocation from a method group
                    formattedCourses.ForEach(Courses.Add);
#pragma warning restore HAA0603 // Delegate allocation from a method group
                }

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Курс добавлен!"));
                EventAggregator.Publish(new CourseAddedOrChangedEvent(true));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                UpdateNoResultsFoundVisibility(Visibility.Collapsed);
            }
            catch (Exception e)
            {
                ErrorText = "Не удалось добавить курс";
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.ClosePane, "Не удалось добавить курс!"));
                EventAggregator.Publish(new CourseAddedOrChangedEvent(false));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }

        private async Task SendCoursePutRequest(List<CourseAttachment> content)
        {
            var putResponse = ( await (App.Address + "Education/Courses")
                                    .SendRequestAsync(content, HttpRequestType.Put, App.Account.GetJwt()))
                                    .DeserializeJson<bool>();

            if (putResponse)
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        _selectedCourse.Course.Title = CourseTitle;
                        _selectedCourse.Course.Description = CourseDescription;
                        _selectedCourse.Attachments = new ObservableCollection<Attachment>(Files);
                    });
            }
        }

        [Command]
        private async void OpenFile(object fileObj)
        {
            if (fileObj is not Attachment file) return;

            await file.OpenFile();
        }

        

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
            var convertedFile = file as Attachment;
            Files.Remove(convertedFile);
            UpdateAccessibility();
        }

        private void UpdateAccessibility()
        {
            IsClearButtonEnabled = Files.Count > 0;
        }


        private async Task<List<Attachment>> ReadFiles(IReadOnlyList<Windows.Storage.IStorageItem> items)
        {
            var result = new List<Attachment>();
            var tasks = new List<Task>();
            for (int i = 0; i < items.Count; i++)
            {
                IStorageItem item = items[i];
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

                    var attachment = new Attachment
                    {
                        Title = file.Name,
                        Data = data
                    };

                    attachment.IdType = attachment.ConvertFileType(file.FileType);

                    result.Add(attachment);

                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                }
            }

            await result.WriteAttachmentImagePath();

            return result;
        }

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
            if (courseObj is not FormattedCourse course) return;

            try
            {
                var response = (await (App.Address + $"Education/Courses.id={course.Course.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (response)
                {
                    Courses.Remove(course);
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Курс удален!"));
                }

                if (Courses.Count == 0)
                {
                    UpdateNoResultsFoundVisibility(Visibility.Visible);
                }
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }
        }

        public async void OnEvent(DropCompletedEvent eventData)
        {
            var items = eventData.Items;

            try
            {
                var files = await ReadFiles(items);

                foreach (var file in files)
                {
                    Files.Add(file);
                }

                UpdateAccessibility();
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }
        }
    }
}
