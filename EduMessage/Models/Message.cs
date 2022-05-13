using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EduMessage.Annotations;

#nullable disable

namespace SignalIRServerTest.Models
{
    public partial class Message : INotifyPropertyChanged
    {
        private string _messageContent;

        public Message()
        {
            MessageAttachments = new HashSet<MessageAttachment>();
        }

        public int Id { get; set; }

        public string MessageContent
        {
            get => _messageContent;
            set
            {
                if (value == _messageContent) return;
                _messageContent = value;
                OnPropertyChanged();
            }
        }

        public DateTime SendDate { get; set; }
        public int? IdAttachments { get; set; }
        public int IdUser { get; set; }
        public int? IdRecipient { get; set; }
        public bool IsChanged { get; set; }
        public int? IdConversation { get; set; }

        public virtual Conversation IdConversationNavigation { get; set; }
        public virtual User IdRecipientNavigation { get; set; }
        public virtual User IdUserNavigation { get; set; }
        [JsonIgnore]
        public virtual ICollection<MessageAttachment> MessageAttachments { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
