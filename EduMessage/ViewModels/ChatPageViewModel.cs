using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class ChatPageViewModel : IEventSubscriber<ReplySentEvent>
    {
        [Property] private User _user;
        [Property] private string _message;
        [Property] private ObservableCollection<Message> _messages;
        [Property] private Visibility _noResultsVisualVisibility;

        private IChat _chat;

        public void Initialize(User user, IChat chat)
        {
            _user = user;
            _chat = chat;
            _messages = new ObservableCollection<Message>();
            NoResultsVisualVisibility = Visibility.Visible;

            _chat.SetOnMethod<string, User>("ReceiveForMe", async (m, u) =>
            {
                Messages.Add(new Message
                {
                    IdRecipient = App.Account.User.Id,
                    IdUser = u.Id,
                    MessageContent = m
                });
                NoResultsVisualVisibility = Visibility.Collapsed;
            });
        }

        [Command]
        private async void SendMessage()
        {
            try
            {
                Messages.Add(new Message
                {
                    IdRecipient = _user.Id,
                    IdUser = App.Account.User.Id,
                    MessageContent = Message
                });
                NoResultsVisualVisibility = Visibility.Collapsed;

                await _chat.SendMessage("SendToUser", _user.Id, _message);

                Message = string.Empty;
            }
            catch (System.Exception e)
            {

            }
        }

        public void OnEvent(ReplySentEvent eventData)
        {
            var userId = eventData.recipientId;
            var message = eventData.Message;
            Messages.Add(new Message
            {
                IdRecipient = userId,
                IdUser = App.Account.User.Id,
                MessageContent = message
            });
        }
    }
}