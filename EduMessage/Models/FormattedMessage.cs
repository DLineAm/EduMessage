using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EduMessage.Annotations;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    public struct FormattedMessage : INotifyPropertyChanged
    {
        private Message _message;
        private HashSet<Attachment> _attachments;

        public Message Message
        {
            get => _message;
            set
            {
                _message = value;
                OnPropertyChanged();
            }
        }

        public HashSet<Attachment> Attachments
        {
            get => _attachments;
            set
            {
                _attachments = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}