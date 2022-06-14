using EduMessage.Annotations;
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.Storage.Pickers;
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
        [Property] private Visibility _userTeacherInputVisibility = Visibility.Collapsed;
        [Property] private GridLength _gridWidth;
        [Property] private string _errorText;
        [Property] private bool _isInfoBarOpen;
        [Property] private ObservableCollection<CourseAttachment> _taskAttachments = new();
        [Property] private CourseTask _currentCourseTask;
        [Property] private int? _mark;
        [Property] private string _taskStatus;
        [Property] private string _dialogActionText;
        [Property] private string _dialogTaskComment;
        [Property] private Visibility _dialogCompletedTaskInputVisibility;

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
            UserTeacherInputVisibility = App.Account.GetUser().IdRole is 1 or 2 ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                var response = (await (App.Address + $"Education/Courses.IdMainCourse={mainCourse.Id}&WithoutUsers={true}")
                    .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<HashSet<CourseAttachment>>()
                    .OrderBy(c => c.IdCourseNavigation.Position);

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
        private async void MoveCourseUp(object parameter)
        {
            if (parameter is not Course courseFromParameter)
            {
                return;
            }

            await MoveCourse(courseFromParameter, false);
        }

        [Command]
        private async void MoveCourseDown(object parameter)
        {
            if (parameter is not Course courseFromParameter)
            {
                return;
            }

            await MoveCourse(courseFromParameter, true);
        }

        private async Task MoveCourse(Course courseFromParameter, bool isAscending)
        {
            var (courseFromParameterPosition, (changedCourseId, changedCoursePosition)) =
                await SendMoveCourseRequest(courseFromParameter.Id, isAscending);

            if (courseFromParameterPosition == 0 && changedCourseId == 0 && changedCoursePosition == 0)
            {
                return;
            }

            courseFromParameter.Position = courseFromParameterPosition;

            var changedCourse = Courses.FirstOrDefault(c => c.Course.Id == changedCourseId);

            if (changedCourse == null)
            {
                return;
            }

            changedCourse.Course.Position = changedCoursePosition;

            Courses = new ObservableCollection<FormattedCourse>(Courses.OrderBy(c => c.Course.Position));
        }

        private async Task<KeyValuePair<int, KeyValuePair<int, int>>> SendMoveCourseRequest(int courseId, bool isAscending)
        {
            try
            {
                var response = (await (App.Address + $"Education/Courses/ChangePosition.CourseId={courseId}&IsAscending={isAscending}")
                            .SendRequestAsync("", HttpRequestType.Put, App.Account.GetJwt()))
                        .DeserializeJson<KeyValuePair<int, KeyValuePair<int, int>>>();
                return response;
            }
            catch (Exception e)
            {
                return new KeyValuePair<int, KeyValuePair<int, int>>();
            }
        }

        [Command]
        private async void GetUserTask(object parameter)
        {
            if (parameter is not int id || id == 0)
            {
                return;
            }

            SetErrorText();
            var user = App.Account.GetUser();

            if (user.IdRole == 2)
            {
                EventAggregator.Publish(new SelectedSpecialityChangedEvent(id));
                return;
            }

            await GetTask(id);
        }

        private async Task GetTask(int courseId)
        {
            var userId = App.Account.GetUser().Id;

            var currentCourse = Courses.FirstOrDefault(c => c.Course.Id == courseId);

            _selectedCourse = currentCourse;

            CurrentCourseTask = currentCourse.Course.IdCourseTaskNavigation;

            TaskAttachments.Clear();

            try
            {
                var response = (await (App.Address + $"Education/Courses/Tasks.IdUser={userId}&IdCourse={courseId}")
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
                    DialogCompletedTaskInputVisibility = Visibility.Visible;
                    return;
                }

                Mark = firstCourseAttachment.Mark;
                TaskStatus = Mark == null ? "Не проверено" : $"Проверено ({Mark})";
                DialogActionText = "Изменить";
                DialogTaskComment = firstCourseAttachment.Comment;
                DialogCompletedTaskInputVisibility = Mark == null ? Visibility.Visible : Visibility.Collapsed;
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
        private async void ApplyTask()
        {
            if (TaskAttachments.Count == 0)
            {
                SetErrorText("Количество вложений должно быть больше 0");
                return;
            }

            try
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Отпарвка задания"));

                var response = (await (App.Address + "Education/Tasks/Add")
                        .SendRequestAsync(TaskAttachments, HttpRequestType.Post, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!response)
                {
                    SetErrorText("При отправлении задания произошла ошибка, повторите отправку задания позднее");
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                    return;

                }

                EventAggregator.Publish(new DialogStatusChanged(true));
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Задание отправлено!"));
            }
            catch (Exception e)
            {

            }
            finally
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }

        }

        //[Command]
        //private async void Apply()
        //{
        //    if (TaskAttachments.Count == 0)
        //    {
        //        SetErrorText("Количество вложений должно быть больше 0");
        //        return;
        //    }

        //    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Отпарвка задания"));

        //    try
        //    {
        //        var response = (await (App.Address + "Education/Courses")
        //                .SendRequestAsync(TaskAttachments, HttpRequestType.Put, App.Account.GetJwt()))
        //            .DeserializeJson<bool>();

        //        if (!response)
        //        {
        //            SetErrorText("Не удалось отправить задание");
        //            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
        //        }

        //        EventAggregator.Publish(new DialogStatusChanged(true));
        //        EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Задание отправлено!"));
        //    }
        //    catch (Exception e)
        //    {
        //        SetErrorText("Не удалось отправить задание");
        //    }
        //    finally
        //    {
        //        EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
        //    }
        //}

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
        private void OpenJournalPage()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedEvent((_mainCourse.Id, _mainCourse)));
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
                    Courses = new ObservableCollection<FormattedCourse>(Courses.OrderBy(c => c.Course.Position));
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

    public class FormattedCourseTask : INotifyPropertyChanged
    {
        private byte? _generalMarkForInterface;
        private string _commentForInterface;
        public User User { get; set; }
        public List<CourseAttachment> Attachments { get; set; }
        public byte? GeneralMark { get; set; }
        public DateTime? SendTime { get; set; }
        public string Comment { get; set; }

        public string CommentForInterface
        {
            get => _commentForInterface;
            set
            {
                _commentForInterface = value;
                OnPropertyChanged();
            }
        }

        public byte? GeneralMarkForInterface
        {
            get => _generalMarkForInterface;
            set
            {
                _generalMarkForInterface = value;
                OnPropertyChanged();
            }
        }

        public FormattedCourseTask(User user, List<CourseAttachment> attachments)
        {
            User = user;
            Attachments = attachments;

            foreach (var courseAttachment in Attachments)
            {
                var mark = courseAttachment.Mark;
                if (courseAttachment.Mark == null) continue;
                GeneralMark = mark;
                GeneralMarkForInterface = mark;
                SendTime = courseAttachment.SendTime;
                Comment = courseAttachment.Comment;
                if (Comment == null) return;
                CommentForInterface = Comment;
                return;
            }

            GeneralMark = 2;
        }

        public KeyValuePair<User, List<CourseAttachment>> GetKeyValuePair()
        {
            return new KeyValuePair<User, List<CourseAttachment>>(User, Attachments);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
