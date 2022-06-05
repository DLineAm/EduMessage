using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(FeatureCollection))]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(INotificator))]
    public partial class ThemeConstructorPageViewModel
    {
        [Property] private List<IFeature> _features;
        [Property] private string _title;
        [Property] private string _description = "";
        [Property] private ObservableCollection<Attachment> _images = new();
        [Property] private ObservableCollection<Attachment> _otherFiles = new();
        [Property] private int _selectedPivotIndex;
        [Property] private string _errorText;
        [Property] private string _taskDescription;
        [Property] private DateTime _taskDate = DateTime.Now.Date;
        [Property] private TimeSpan _taskTime = new(DateTime.Now.Add(new TimeSpan(1, 0, 0)).Hour, 0 ,0);
        [Property] private CourseTask _task;
        [Property] private int _filesCount;
        [Property] private InfoBarSeverity _severity = InfoBarSeverity.Informational;
        [Property] private string _infoBarTitle = "Информация";
        [Property] private string _infoBarMessage = "Очистите поле ввода описания для удаление текущего задания";

        private IValidator<string[]> _validator;

        private FormattedCourse _formattedCourse;

        private int _mainCourseId;

        private bool _isCourseAddMode;
        

        public void Initialize(int mainCourseId)
        {
            _isCourseAddMode = true;
            BaseInitialize(mainCourseId);
        }

        private void BaseInitialize(int mainCourseId)
        {
            _features = FeatureCollection.Features.Where(f => f.ShowInBar).ToList();
            _validator = ControlContainer.Get().Resolve<IValidator<string[]>>();
            _mainCourseId = mainCourseId;
            Images.CollectionChanged += OnCollectionChanged;
            OtherFiles.CollectionChanged += OnCollectionChanged;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            FilesCount = Images.Count + OtherFiles.Count;
        }

        private void ChangeInfoBarInformation(InfoBarSeverity severity, string message = null)
        {
            Severity = severity;
            InfoBarMessage = message ?? "Очистите поле ввода описания для удаление текущего задания";
            InfoBarTitle = severity == InfoBarSeverity.Informational ? "Информация" : "Ошибка";
        }

        [Command]
        private void AddOrChangeTask()
        {
            if (string.IsNullOrWhiteSpace(TaskDescription))
            {
                if (Task != null)
                {
                    Task = null;
                    EventAggregator.Publish(new DialogStatusChanged(true));
                    return;
                }
                ChangeInfoBarInformation(InfoBarSeverity.Error, "При добавлении нового задания поле описания должно быть заполнено!");
                //DialogErrorText = "Поле описания должно быть заполнено";
                return;
            }

            var resultTaskDate = _taskDate.Add(_taskTime);

            if (resultTaskDate < DateTime.Now)
            {
                ChangeInfoBarInformation(InfoBarSeverity.Error, "Выбранные дата и время должны быть больше текущих!");
                //DialogErrorText = "Выбранные дата и время должны быть больше текущих";
                return;
            }

            Task ??= new CourseTask();

            Task.EndTime = resultTaskDate;
            Task.Description = TaskDescription;

            ChangeInfoBarInformation(InfoBarSeverity.Informational);

            EventAggregator.Publish(new DialogStatusChanged(true));
        }

        [Command]
        private async void Apply()
        {
            ErrorText = "";
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Сохранение курса"));

            var validatorResponse = _validator.Validate(new []{Title, Description});

            if (!validatorResponse)
            {
                ErrorText = "Все поля дожны быть заполнены!";
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось добавить курс!"));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                return;
            }

            var attachments = Images.ToList();
            attachments.AddRange(OtherFiles);

            var taskId = Task?.Id;

            var course = new Course
            {
                Id = _formattedCourse?.Course?.Id ?? 0,
                Title = Title,
                Description = Description,
                IdMainCourse = _mainCourseId,
                IdTeacher = App.Account.GetUser().Id,
                IdCourseTaskNavigation = Task,
                IdTask = taskId
            };

            var courseAttachments = new List<CourseAttachment>();

            foreach (var attachment in attachments)
            {
                var id = attachment.Id;
                courseAttachments.Add(new CourseAttachment
                {
                    IdCourse = course.Id == 0 ? null : course.Id,
                    IdCourseNavigation = course,
                    IdAttachmanentNavigation = id == 0 ? attachment : null,
                    IdAttachmanent = id == 0 ? null : id
                });
            }

            if (courseAttachments.Count == 0)
            {
                courseAttachments.Add(new CourseAttachment
                {
                    IdCourseNavigation = course
                });
            }

            try
            {
                if (!_isCourseAddMode)
                {
                    await SendCoursePutRequest(courseAttachments);
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Курс изменен!"));
                    EventAggregator.Publish(new CourseAddedOrChangedEvent(true));
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                    EventAggregator.Publish(new EducationPageBack());
                    return;
                }
                var (courseId, attachmentIds) = (await (App.Address + "Education/Courses.FromList")
                           .SendRequestAsync(courseAttachments, HttpRequestType.Post, App.Account.GetJwt()))
                       .DeserializeJson<KeyValuePair<int, List<int>>>();

                if (courseId != -1)
                {
                    course.Id = courseId;
                    for (int i = 0; i < attachmentIds.Count; i++)
                    {
                        var attachmentId = attachmentIds[i];
                        var courseAttachment = courseAttachments[i];
                        courseAttachment.Id = attachmentId;
                        courseAttachment.IdCourse = courseId;
                    }
                }
                else
                {
                    throw new Exception();
                }

                EventAggregator.Publish(new CourseRequestCompleted(courseAttachments));

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Курс добавлен!"));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));

                try
                {
                    EventAggregator.Publish(new EducationPageBack());
                }
                catch (Exception e)
                {

                }
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось добавить курс!"));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }

        private async Task SendCoursePutRequest(List<CourseAttachment> content)
        {
            var putResponse = ( await (App.Address + "Education/Courses")
                    .SendRequestAsync(content, HttpRequestType.Put, App.Account.GetJwt()))
                .DeserializeJson<bool>();
        }

        [Command]
        private void Back()
        {
            EventAggregator.Publish(new EducationPageBack());
        }

        [Command]
        private void AddString(object parameter)
        {
            if (parameter is not IFeature feature)
            {
                return;
            }

            var patternString = feature.GetString();
            if (_description.Length != 0 && _description.Last() != ' ')
            {
                patternString = patternString.Insert(0, " ");
            }

            Description += patternString;

            EventAggregator.Publish(new FeatureAdded(Description.Length - 2));
        }

        [Command]
        private void ChangeTask()
        {
            ChangeInfoBarInformation(InfoBarSeverity.Informational);
            EventAggregator.Publish(new CourseTaskDialogOpedRequested());
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

            if (Images.Count + OtherFiles.Count + filesCount > 10)
            {
                Notificator.Notificate("Ошибка", "Количество вложений должно быть не более 10");
                return;
            }

            var attachments = await files.CreateAttachments();
            foreach (var attachment in attachments)
            {
                if (attachment.IdType == 3)
                {
                    Images.Add(attachment);
                    return;
                }

                OtherFiles.Add(attachment);
            }
        }

        [Command]
        private void ClearFiles()
        {
            if (SelectedPivotIndex == 0)
            {
                Images.Clear();
                return;
            }

            OtherFiles.Clear();
        }

        [Command]
        private void DeleteFile(object parameter)
        {
            if (parameter is not Attachment attachment)
            {
                return;
            }

            if (attachment.IdType == 3)
            {
                Images.Remove(attachment);
                return;
            }

            OtherFiles.Remove(attachment);
        }

        public async Task Initialize(int mainCourseId, FormattedCourse course)
        {
            //var id = course.Course.Id;
            _isCourseAddMode = false;
            _formattedCourse = course;
            Title = course.Course.Title;
            Description = course.Course.Description;
            var images = course.Attachments.Where(a => a is {IdType: 3});
            foreach (var attachment in images)
            {
                await attachment.SplitAndGetImage(64);
            }
            Images = new ObservableCollection<Attachment>(images);
            OtherFiles = new ObservableCollection<Attachment>(course.Attachments.Where(a => a != null && a.IdType != 3));
            FilesCount = Images.Count + OtherFiles.Count;
            Task = course.Course.IdCourseTaskNavigation;
            TaskDescription = Task?.Description;
            BaseInitialize(mainCourseId);
            var date = Task?.EndTime;
            if (date == null)
            {
                return;
            }
            TaskDate = date.Value.Date;
            TaskTime = new TimeSpan(date.Value.Hour, date.Value.Minute, 0);
        }
    }

    public record DialogStatusChanged(bool IsSuccess);

    public record CourseTaskDialogOpedRequested;

    public record EducationPageBack;

    public record CourseRequestCompleted(IEnumerable<CourseAttachment> CourseAttachments);
}