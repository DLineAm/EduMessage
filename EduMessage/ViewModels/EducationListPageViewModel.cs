using EduMessage.Models;
using EduMessage.Services;

using Mapster;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(TypeAdapterConfig))]
    public partial class EducationListPageViewModel : IEventSubscriber<CourseRequestCompleted>
    {
        [Property] private ObservableCollection<FormattedCourse> _courses = new();
        [Property] private ObservableCollection<Attachment> _files = new();
        [Property] private bool _isClearButtonEnabled;
        [Property] private string _courseTitle;
        [Property] private string _courseDescription;
        [Property] private Visibility _noResultsFoundAnimationVisibility = Visibility.Collapsed;
        [Property] private Visibility _teacherInputVisibility;
        [Property] private Visibility _userInputVisibility = Visibility.Collapsed;
        [Property] private GridLength _gridWidth;
        [Property] private string _errorText;
        [Property] private bool _isInfoBarOpen;
        [Property] private ObservableCollection<CourseAttachment> _taskAttachments = new();
        [Property] private CourseTask _currentCourseTask;
        [Property] private int? _mark;
        [Property] private string _taskStatus;
        [Property] private string _dialogActionText;

        private bool _isCourseAddMode;
        private FormattedCourse _selectedCourse;
        private MainCourse _mainCourse;

        public async Task Initialize(object parameter)
        {
            if (parameter is not MainCourse mainCourse)
            {
                return;
            }
            _mainCourse = mainCourse;

            UpdateTeacherInputVisibility(App.Account.GetUser().IdRole == 2 ? Visibility.Visible : Visibility.Collapsed);
            UserInputVisibility = App.Account.GetUser().IdRole == 1 ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                var response = (await (App.Address + $"Education/Courses.IdMainCourse={mainCourse.Id}")
                    .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<HashSet<CourseAttachment>>();

                Courses = new ObservableCollection<FormattedCourse>();

                var groupedCourseAttachments = response.GroupBy(ca => ca.IdCourse);

                foreach (var groupedCourseAttachment in groupedCourseAttachments)
                {
                    var key = groupedCourseAttachment.Key;
                    var courses = response.Where(c => c.IdCourse == key);
                    var formattedCourse = courses.Adapt<FormattedCourse>(TypeAdapterConfig);
                    await formattedCourse.Attachments.WriteAttachmentImagePath(700);
                    Courses.Add(formattedCourse);
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
        private async void GetUserTask(object parameter)
        {
            if (parameter is not int id || id == 0)
            {
                return;
            }

            SetErrorText();

            var userId = App.Account.GetUser().Id;

            var currentCourse = Courses.FirstOrDefault(c => c.Course.Id == id);

            _selectedCourse = currentCourse;

            CurrentCourseTask = currentCourse.Course.IdCourseTaskNavigation;

            TaskAttachments.Clear();

            try
            {
                var response = (await (App.Address + $"Education/Courses/Tasks.IdUser={userId}&IdCourse={id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<IEnumerable<CourseAttachment>>();

                foreach (var courseAttachment in response)
                {
                    await courseAttachment.IdAttachmanentNavigation.SplitAndGetImage(64);
                    courseAttachment.IdCourseNavigation = null;
                    TaskAttachments.Add(courseAttachment);
                }

                var firstCourseAttachment = response.FirstOrDefault();
                if (firstCourseAttachment == null)
                {
                    DialogActionText = "Выполнить";
                    TaskStatus = "Не выполнено";
                    return;
                }

                Mark = firstCourseAttachment.Mark;
                TaskStatus = Mark == null ? "Не проверено" : $"Проверено ({Mark})";
                DialogActionText = "Изменить";
            }
            catch (Exception e)
            {
                
            }
        }


        private void SetErrorText(string text = null)
        {
            ErrorText = text;
            if (text == null)
            {
                IsInfoBarOpen = false;
                return;
            }

            IsInfoBarOpen = true;
        }

        [Command]
        private void InitializeChangeCourseDialog(object courseObj)
        {
            var course = courseObj as FormattedCourse;
            EventAggregator.Publish(new SelectedSpecialityChangedEvent((_mainCourse.Id, course)));
        }

        [Command]
        private async void Apply()
        {
            if (TaskAttachments.Count == 0)
            {
                SetErrorText("Количество вложений должно быть больше 0");
                return;
            }

            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Отпарвка задания"));

            try
            {
                var response = (await (App.Address + "Education/Courses")
                        .SendRequestAsync(TaskAttachments, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!response)
                {
                    SetErrorText("Не удалось отправить задание");
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                }

                EventAggregator.Publish(new DialogStatusChanged(true));
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Задание отправлено!"));
            }
            catch (Exception e)
            {
                SetErrorText("Не удалось отправить задание");
            }
            finally
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }

        [Command]
        private async void OpenFile(object fileObj)
        {
            if (fileObj is not Attachment file) return;

            await file.OpenFile();
        }

        [Command]
        private void DeleteFile(object fileObj)
        {
            if (fileObj is not CourseAttachment attachment)
            {
                return;
            }

            TaskAttachments.Remove(attachment);
        }

        [Command]
        private void ClearFiles()
        {
            TaskAttachments.Clear();
        }

        [Command]
        private async void AddFiles()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            var files = await picker.PickMultipleFilesAsync();

            var filesCount = files.Count;

            if (TaskAttachments.Count  + filesCount > 10)
            {
                SetErrorText("Количество вложений должно быть не более 10");
                return;
            }

            
            foreach (var attachment in await files.CreateAttachments())
            {
                var courseAttachment = new CourseAttachment
                {
                    IdCourse = _selectedCourse.Course.Id,
                    IdAttachmanentNavigation = attachment,
                    IdUser = App.Account.GetUser().Id,
                    //IdCourseNavigation = _selectedCourse.Course
                };
                TaskAttachments.Add(courseAttachment);
            }
        }


        [Command]
        private void InitializeAddCourseDialog()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedEvent((_mainCourse.Id, new FormattedCourse())));
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


        public void OnEvent(CourseRequestCompleted eventData)
        {
            var courses = eventData.CourseAttachments
                .Adapt<FormattedCourse>(TypeAdapterConfig);

            Courses.Add(courses);
        }
    }
}
