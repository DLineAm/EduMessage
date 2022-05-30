using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        private IValidator<string[]> _validator;

        private FormattedCourse _formattedCourse;

        private int _mainCourseId;

        public void Initialize(int specialityId)
        {
            _features = FeatureCollection.Features.Where(f => f.ShowInBar).ToList();
            _validator = ControlContainer.Get().Resolve<IValidator<string[]>>();
            _mainCourseId = specialityId;
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
                return;
            }

            var attachments = Images.ToList();
            attachments.AddRange(OtherFiles);

            var course = new Course
            {
                Title = Title,
                Description = Description,
                IdMainCourse = _mainCourseId
            };

            var courseAttachments = new List<CourseAttachment>();

            foreach (var attachment in attachments)
            {
                courseAttachments.Add(new CourseAttachment
                {
                    IdCourseNavigation = course,
                    IdAttachmanentNavigation = attachment
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

                EventAggregator.Publish(new EducationPageBack());
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.ClosePane, "Не удалось добавить курс!"));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
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
    }

    public record EducationPageBack;

    public record CourseRequestCompleted(IEnumerable<CourseAttachment> CourseAttachments);
}