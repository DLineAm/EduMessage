using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(INotificator))]
    public partial class ChatPageViewModel : IEventSubscriber<ReplySentEvent>
    {
        [Property] private User _user;
        [Property] private string _message;
        [Property] private ObservableCollection<MessageList> _messages = new();
        [Property] private Visibility _noResultsVisualVisibility;
        [Property] private ObservableCollection<Attachment> _messageAttachments = new();
        [Property] private bool _isFilesBorderCollapsed = true;
        [Property] private bool _isRefactorBorderCollapsed = true;
        private UserConversation _conversation;
        private FormattedMessage _selectedFormattedMessage;

        private IChat _chat;

        public async Task Initialize(UserConversation conversation, IChat chat)
        {
            var user = conversation.IdUserNavigation;
            _conversation = conversation;
            _user = user;
            _chat = chat;

            try
            {
                var response = (await (App.Address + $"Message/Id={conversation.IdConversation}")
                        .SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                    .DeserializeJson<List<MessageAttachment>>();

                var alreadyExistedMessageIds = new List<int>();
                var messages = response.Select(messageAttachment => messageAttachment.IdMessageNavigation).ToList();

                foreach (var message in messages)
                {
                    if (alreadyExistedMessageIds.Any(m => m == message.Id))
                    {
                        continue;
                    }
                    var attachments = response.Where(m => m.IdMessage == message.Id)
                        .Select(m => m.IdAttachmentNavigation);
                    await attachments.WriteAttachmentImagePath();
                    var formattedMessage = new FormattedMessage { Message = message, Attachments = attachments.ToList() };
                    alreadyExistedMessageIds.Add(message.Id);

                    AddToMessagesWithGrouping(formattedMessage);
                }


            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }

            if (Messages.Count == 0)
            {
                NoResultsVisualVisibility = Visibility.Visible;
            }

            _chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", (m, u) =>
            {
                var message = m.FirstOrDefault().IdMessageNavigation;
                var formattedMessage = new FormattedMessage { Message = message, Attachments = new List<Attachment>() };
                foreach (var attachment in m.Select(messageAttachment => messageAttachment.IdAttachmentNavigation)
                             .Where(attachment => attachment != null))
                {
                    formattedMessage.Attachments.Add(attachment);
                }

                AddToMessagesWithGrouping(formattedMessage);
            });
        }

        private void UpdateAttachmentsListVisibility()
        {
            IsFilesBorderCollapsed = MessageAttachments.Count == 0;
        }

        [Command]
        private async void OpenFile(object parameter)
        {
            if (parameter is Attachment attachment)
            {
                await attachment.OpenFile();
            }
        }

        [Command]
        private async void OpenFileDialog()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.Downloads
            };
            picker.FileTypeFilter.Add("*");

            var files = await picker.PickMultipleFilesAsync();

            var filesCount = files.Count;

            if (MessageAttachments.Count + filesCount > 10)
            {
                //new ToastNotificator().Notificate();
                Notificator.Notificate("Ошибка", "Количество вложоений должно быть не более 10");
                return;
            }

            var result = new List<Attachment>();

            foreach (var file in files)
            {
                var data = await FileIO.ReadBufferAsync(file);
                var title = file.Name;

                await Task.Delay(TimeSpan.FromMilliseconds(1));

                var attachment = new Attachment
                {
                    Title = title,
                    Data = data.ToArray()
                };

                attachment.IdType = attachment.ConvertFileType(file.FileType);

                result.Add(attachment);
            }

            await result.WriteAttachmentImagePath();

            result.ForEach(MessageAttachments.Add);
            UpdateAttachmentsListVisibility();
        }
        

        private void AddToMessagesWithGrouping(FormattedMessage formattedMessage)
        {
            var groupParameter = formattedMessage.Message.SendDate.Date;
            var foundMessageGroup = Messages.FirstOrDefault(m => (DateTime)m.Key == groupParameter);
            if (foundMessageGroup == null)
            {
                Messages.Add(new MessageList(new[] { formattedMessage }) { Key = groupParameter });
                NoResultsVisualVisibility = Visibility.Collapsed;
                return;
            }
            foundMessageGroup.Add(formattedMessage);
            NoResultsVisualVisibility = Visibility.Collapsed;
        }

        [Command]
        private void RemoveAttachment(object parameter)
        {
            if (parameter is not Attachment attachment) return;

            MessageAttachments.Remove(attachment);
            UpdateAttachmentsListVisibility();
        }

        [Command]
        private void CopyToClipboard(object parameter)
        {
            var message = _selectedFormattedMessage.Message;
            if (message is null) return;

            var package = new DataPackage
            {
                RequestedOperation = DataPackageOperation.Copy
            };
            package.SetText(message.MessageContent);
            Clipboard.SetContent(package);

            EventAggregator.Publish(new InAppNotificationShowing(Symbol.Like,
                "Текст сообщения скопирован в буфер обмена!"));
        }

        [Command]
        private void StartRefactorMode()
        {
            var message = _selectedFormattedMessage.Message;
            if (message is null) return;

            var attachments = _selectedFormattedMessage.Attachments
                .Where(a => a != null)
                .ToList();
            MessageAttachments.Clear();
            attachments.ForEach(MessageAttachments.Add);
            UpdateAttachmentsListVisibility();

            Message = message.MessageContent;

            IsRefactorBorderCollapsed = false;
        }

        [Command]
        private void CancelRefactorMode()
        {
            Message = string.Empty;
            MessageAttachments.Clear();
            IsRefactorBorderCollapsed = true;
            UpdateAttachmentsListVisibility();
        }

        [Command]
        private void SetSelectedItem(object parameter)
        {
            if (parameter is not FormattedMessage formattedMessage) return;
            _selectedFormattedMessage = formattedMessage;
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
                    SendDate = DateTime.Now,
                    IdConversation = _conversation.IdConversation
                };
                var list = new List<MessageAttachment>();

                foreach (var attachment in MessageAttachments)
                {
                    var messageAttachment = new MessageAttachment
                    {
                        IdMessageNavigation = message,
                        IdAttachmentNavigation = attachment
                    };
                    list.Add(messageAttachment);
                }

                var formattedMessage = new FormattedMessage
                {
                    Message = message
                };

                if (MessageAttachments.Count == 0)
                {
                    var messageAttachment = new MessageAttachment
                        { IdMessageNavigation = message };
                    list.Add(messageAttachment);
                }
                if (MessageAttachments.Count != 0)
                {
                    var attachments = MessageAttachments.ToList();
                    await attachments.WriteAttachmentImagePath();
                    formattedMessage.Attachments = attachments;
                }

                AddToMessagesWithGrouping(formattedMessage);

                await _chat.SendMessage("SendToUser", _user.Id, list);

                Message = string.Empty;
                MessageAttachments.Clear();
                UpdateAttachmentsListVisibility();
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (System.Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }
        }

        [Command]
        private async void DeleteMessage()
        {
            if (_selectedFormattedMessage.Message is null) return;

            for (var index = 0; index < Messages.Count; index++)
            {
                var messageList = Messages[index];
                messageList.Remove(_selectedFormattedMessage);
                if (messageList.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    Messages.Remove(messageList);
                }
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
            AddToMessagesWithGrouping(formattedMessage);
        }
    }

    public class MessageList : ObservableCollection<FormattedMessage>
    {
        public MessageList(IEnumerable<FormattedMessage> items) : base(items)
        {

        }

        public object Key { get; set; }
    }

    public struct FormattedMessage
    {

        public Message Message { get; set; }
        public List<Attachment> Attachments { get; set; }
    }

    public record InAppNotificationShowing(Symbol Icon, string Text);

}