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
using System.Threading;
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
        [Property] private Visibility _flyoutMenuItemsVisibility;

        private UserConversation _conversation;
        private FormattedMessage _selectedFormattedMessage;
        private SynchronizationContext _context;
        private event EventHandler<int> AddedMessageIdReceived;

        private IChat _chat;

        public async Task Initialize(UserConversation conversation, IChat chat)
        {
            var user = conversation.IdUserNavigation;
            _conversation = conversation;
            _user = user;
            _chat = chat;

            try
            {
                var response = (await (App.Address + $"Message/Id=" + conversation.IdConversation.ToString())
                        .SendRequestAsync("", HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<HashSet<MessageAttachment>>()
                    .OrderBy(ma => ma.IdMessageNavigation.SendDate);

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

                    var enumerable = attachments as Attachment[] ?? attachments.ToArray();
                    await enumerable.WriteAttachmentImagePath();

                    var formattedMessage = new FormattedMessage { Message = message, Attachments = new HashSet<Attachment>(enumerable) };
                    alreadyExistedMessageIds.Add(message.Id);

                    _context = SynchronizationContext.Current;
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

            _chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", async (m, u) =>
            {
                var message = m.FirstOrDefault()?.IdMessageNavigation;
                var formattedMessage = new FormattedMessage { Message = message, Attachments = new HashSet<Attachment>() };
                foreach (var messageAttachment in m)
                {
                    var attachment = messageAttachment.IdAttachmentNavigation;
                    if (attachment != null)
                    {
                        attachment.ImagePath = await attachment.Data.CreateBitmap();
                        formattedMessage.Attachments.Add(attachment);
                    }
                }

                AddToMessagesWithGrouping(formattedMessage);
            });

            _chat.SetOnMethod<int>("ReceiveAddedMessage", messageId =>
            {
                AddedMessageIdReceived?.Invoke(null, messageId);
            });

            _chat.SetOnMethod<int>("ReceiveDeletedMessage", messageId =>
            {
                foreach (var messageList in Messages)
                {
                    var formattedMessage = messageList.FirstOrDefault(m => m.Message.Id == messageId);
                    messageList.Remove(formattedMessage);
                    if (!messageList.Any())
                    {
                        Messages.Remove(messageList);
                    }
                }
            });

            _chat.SetOnMethod<List<MessageAttachment>>("MessageChanged", m =>
            {
                var message = m.FirstOrDefault()?.IdMessageNavigation;
                ReplaceChangedMessage(m, message);
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
                Notificator.Notificate("Ошибка", "Количество вложений должно быть не более 10");
                return;
            }

            var result = new List<Attachment>();

            for (int i = 0; i < files.Count; i++)
            {
                StorageFile file = files[i];
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


        private void AddToMessagesWithGrouping(FormattedMessage formattedMessage, bool checkForExist = false)
        {
            if (checkForExist)
            {
                foreach (var messageList in Messages)
                {
                    foreach (var f in messageList)
                    {
                        if (f.Message.Id == formattedMessage.Message.Id)
                        {
                            return;
                        }
                    }
                }
            }
            _context?.Post(_ =>
            {
                var groupParameter = formattedMessage.Message.SendDate.Date;
                MessageList foundMessageGroup = null;
                foreach (var m in Messages)
                {
                    if ((DateTime)m.Key == groupParameter)
                    {
                        foundMessageGroup = m;
                        break;
                    }
                }

                if (foundMessageGroup == null)
                {
                    Messages.Add(new(new[] { formattedMessage }) { Key = groupParameter });
                    NoResultsVisualVisibility = Visibility.Collapsed;
                    return;
                }
                foundMessageGroup.Add(formattedMessage);
                NoResultsVisualVisibility = Visibility.Collapsed;
            }, null);
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

            EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept,
                "Текст сообщения скопирован в буфер обмена!"));
        }

        [Command]
        private void StartRefactorMode()
        {
            var message = _selectedFormattedMessage.Message;
            if (message is null) return;

            var attachments = _selectedFormattedMessage.Attachments?
                .Where(a => a != null)
                .ToList();
            MessageAttachments.Clear();
            attachments?.ForEach(MessageAttachments.Add);
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

            if (_selectedFormattedMessage.Message.IdUser != App.Account.GetUser().Id)
            {
                FlyoutMenuItemsVisibility = Visibility.Collapsed;
                return;
            }

            FlyoutMenuItemsVisibility = Visibility.Visible;
        }

        [Command]
        private async void SendMessage()
        {
            try
            {
                if (!IsRefactorBorderCollapsed)
                {
                    var messageToChange = _selectedFormattedMessage.Message;
                    messageToChange.IsChanged = true;
                    messageToChange.MessageContent = Message;
                    var attachments = MessageAttachments.ToList();
                    attachments.ForEach(a => a.IdTypeNavigation = null);
                    var messageAttachments = attachments.Select(a => new MessageAttachment
                    {
                        IdMessage = messageToChange.Id,
                        IdMessageNavigation = messageToChange,
                        IdAttachment = a.Id == 0 ? null : a.Id,
                        IdAttachmentNavigation = a
                    }).ToList();

                    if (!messageAttachments.Any())
                    {
                        messageAttachments.Add(new MessageAttachment
                        {
                            IdMessage = messageToChange.Id,
                            IdMessageNavigation = messageToChange,
                        });
                    }

                    var response = (await (App.Address + "Message/Change").SendRequestAsync(messageAttachments,
                            HttpRequestType.Put, App.Account.GetJwt()))
                        .DeserializeJson<bool>();

                    //await _chat.SendMessage("ChangeMessage", messageAttachments);

                    Message = string.Empty;
                    MessageAttachments.Clear();
                    IsRefactorBorderCollapsed = true;
                    UpdateAttachmentsListVisibility();

                    if (response)
                    {
                        await _chat.SendMessage("ChangeMessage", messageToChange.Id);
                        EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Сообщение изменено"));

                        ReplaceChangedMessage(messageAttachments, messageToChange);
                        return;
                    }

                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось изменить сообщение"));

                    return;
                }
                var message = new Message
                {
                    IdRecipient = _user.Id,
                    IdUser = App.Account.GetUser().Id,
                    MessageContent = Message,
                    SendDate = DateTime.Now,
                    IdConversation = _conversation.IdConversation
                };
                void Handler(object sender, int i)
                {
                    message.Id = i;

                    AddedMessageIdReceived -= Handler;
                }
                AddedMessageIdReceived += Handler;
                var list = MessageAttachments.Select(attachment => new MessageAttachment
                {
                    IdMessageNavigation = message,
                    IdAttachmentNavigation = attachment
                })
                    .ToList();

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
                    formattedMessage.Attachments = new HashSet<Attachment>(attachments);
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

        private void ReplaceChangedMessage(List<MessageAttachment> messageAttachments, Message messageToChange)
        {
            messageAttachments.Where(ma => ma.IdAttachmentNavigation != null)
                .ToList()
                .ForEach(async ma => await ma.IdAttachmentNavigation.SplitAndGetImage());

            foreach (var formattedMessages in Messages)
            {
                var foundMessage = new FormattedMessage();
                foreach (var ms in formattedMessages)
                {
                    if (ms.Message.Id == messageToChange?.Id)
                    {
                        foundMessage = ms;
                        break;
                    }
                }

                if (foundMessage.Message == null) continue;

                var index = formattedMessages.IndexOf(foundMessage);
                formattedMessages.RemoveAt(index);
                foundMessage.Message = messageToChange;
                foundMessage.Attachments = new HashSet<Attachment>(messageAttachments.Select(ma => ma.IdAttachmentNavigation));
                formattedMessages.Insert(index, foundMessage);
            }
        }

        [Command]
        private async void DeleteMessage()
        {
            var messageId = _selectedFormattedMessage.Message?.Id;
            //if (message is null) return;

            await _chat.SendMessage("DeleteMessage", messageId);

            for (var index = 0; index < Messages.Count; index++)
            {
                var messageList = Messages[index];
                var messageToDelete = new FormattedMessage();
                foreach (var m in messageList)
                {
                    if (m.Message.Id == messageId)
                    {
                        messageToDelete = m;
                        break;
                    }
                }

                if (messageToDelete.Message == null) continue;
                messageList.Remove(messageToDelete);
                if (messageList.Count != 0) continue;
                await Task.Delay(TimeSpan.FromMilliseconds(500));
                Messages.Remove(messageList);
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
            var userId = eventData.RecipientId;
            var messageList = eventData.Message;
            var message = messageList.FirstOrDefault()?.IdMessageNavigation;
            var formattedMessage = new FormattedMessage
            {
                Message = message,
                Attachments = null
            };

            void Handler(object sender, int i)
            {
                message.Id = i;

                AddedMessageIdReceived -= Handler;
            }
            AddedMessageIdReceived += Handler;

            AddToMessagesWithGrouping(formattedMessage, true);
        }
    }
}