using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using EduMessage.Annotations;
using SignalIRServerTest;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    public class FormattedCourse : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public Course Course { get; set; }

        public List<Attachment> Attachments
        {
            get => _attachments;
            set
            {
                _attachments = value;
                OnPropertyChanged();
                ChangeFileInfoVisibility();
            }
        }

        private Visibility _filesInfoVisibility;
        private List<Attachment> _attachments;

        private void ChangeFileInfoVisibility()
        {
            FilesInfoVisibility = Attachments == null
                                  || Attachments.Count == 0
                                  || Attachments.Count == 1 && Attachments.First() == null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public Visibility FilesInfoVisibility { get => _filesInfoVisibility; set { _filesInfoVisibility = value; OnPropertyChanged(); } }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}