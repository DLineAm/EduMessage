using System;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using SignalIRServerTest.Models;

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

        private FormattedCourse _formattedCourse;

        public void Initialize()
        {
            _features = FeatureCollection.Features.Where(f => f.ShowInBar).ToList();
        }

        [Command]
        private void Back()
        {
            new Navigator().GoBack(FrameType.EducationFrame);
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
}