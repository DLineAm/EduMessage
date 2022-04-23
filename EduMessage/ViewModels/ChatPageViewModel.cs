using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest;
using SignalIRServerTest.Models;
using EduMessage.Pages;
using Windows.UI.Xaml.Media.Animation;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Controls.TextToolbarSymbols;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class ChatPageViewModel : IEventSubscriber<ReplySentEvent>
    {
        [Property] private User _user;
        [Property] private string _message;
        [Property] private ObservableCollection<MessageList> _messages;
        [Property] private Visibility _noResultsVisualVisibility;

        private IChat _chat;

        public void Initialize(User user, IChat chat)
        {
            _user = user;
            _chat = chat;
            _messages = new ObservableCollection<MessageList>();
            NoResultsVisualVisibility = Visibility.Visible;

            _chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", (m, u) =>
            {
                var message = m.FirstOrDefault().IdMessageNavigation;
                var formattedMessage = new FormattedMessage{Message = message, Attachments = new List<Attachment>()};
                foreach (var attachment in m.Select(messageAttachment => messageAttachment.IdAttachmentNavigation)
                             .Where(attachment => attachment != null))
                {
                    formattedMessage.Attachments.Add(attachment);
                }

                var groupParameter = formattedMessage.Message.SendDate.ToString("d");
                var foundMessageGroup = Messages.FirstOrDefault(m => m.Key == groupParameter);
                if (foundMessageGroup == null)
                {
                    Messages.Add(new MessageList(new []{formattedMessage}){Key = groupParameter});
                    NoResultsVisualVisibility = Visibility.Collapsed;
                    return;
                }
                foundMessageGroup.Add(formattedMessage);
                NoResultsVisualVisibility = Visibility.Collapsed;
            });
        }

        [Command]
        private async void SendMessage()
        {
            try
            {
                var message = new Message
                {
                    IdRecipient = _user.Id,
                    IdUser = App.Account.User.Id,
                    MessageContent = Message,
                    SendDate = DateTime.Now
                };
                var attachment = new Attachment {Title = "Test", IdType = 1, Data = new byte[255]};
                var messageAttachment = new MessageAttachment
                    {IdAttachmentNavigation = attachment, IdMessageNavigation = message};
                var list = new List<MessageAttachment> {messageAttachment};
                var formattedMessage = new FormattedMessage
                {
                    Attachments = new List<Attachment> {attachment},
                    Message = message
                };
                var groupParameter = formattedMessage.Message.SendDate.ToString("d");
                var foundMessageGroup = Messages.FirstOrDefault(m => m.Key == groupParameter);
                if (foundMessageGroup == null)
                {
                    Messages.Add(new MessageList(new []{formattedMessage}) {Key = groupParameter});
                    NoResultsVisualVisibility = Visibility.Collapsed;
                    return;
                }
                foundMessageGroup.Add(formattedMessage);
                NoResultsVisualVisibility = Visibility.Collapsed;

                await _chat.SendMessage("SendToUser", _user.Id, list);

                Message = string.Empty;
            }
            catch (System.Exception e)
            {

            }
        }

        [Command]
        private void NavigateToAccountInfo(object parameter)
        {
            if (parameter is User user)
            {
                new Navigator().Navigate(typeof(AccountInfoPage), user, new DrillInNavigationTransitionInfo(), FrameType.MenuFrame);
            }
        }
        public void OnEvent(ReplySentEvent eventData)
        {
            var userId = eventData.recipientId;
            var messageList = eventData.Message;
            var message = messageList.FirstOrDefault().IdMessageNavigation;
            var formattedMessage = new FormattedMessage
            {
                Message = message,
                Attachments = null
            };
            var groupParameter = formattedMessage.Message.SendDate.ToString("d");
            var foundMessageGroup = Messages.FirstOrDefault(m => m.Key == groupParameter);
            if (foundMessageGroup == null)
            {
                Messages.Add(new MessageList(new []{formattedMessage}){Key = groupParameter});
                NoResultsVisualVisibility = Visibility.Collapsed;
                return;
            }
            foundMessageGroup.Add(formattedMessage);
        }
    }

    public class MessageList : ObservableCollection<FormattedMessage>
    {
        public MessageList(IEnumerable<FormattedMessage> items) : base(items)
        {
            
        }

        public string Key { get; set; }
    }

    public struct FormattedMessage
    {

        public Message Message { get; set; }
        public List<Attachment> Attachments { get; set; }
    }
}