using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Microsoft.Toolkit.Uwp.Notifications;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public class ToastNotificator : INotificator
    {
        public async void Notificate(string title, object message)
        {
            if (message is not KeyValuePair<List<MessageAttachment>, User> messagePair)
            {
                return;
            }

            var messageFromPair = messagePair.Key.FirstOrDefault().IdMessageNavigation;
            var conversationId = messageFromPair.IdConversation;
            

            var messageText = messageFromPair.MessageContent;
            var user = messagePair.Value;
            new ToastContentBuilder()
                .AddText(user.FirstName + " " + user.LastName, hintMaxLines: 1)
                .AddAppLogoOverride(new Uri(await SaveImage(user.Image)), ToastGenericAppLogoCrop.Circle)
                .AddText(messageText)
                .AddInputTextBox("tbReply", "Напишите сообщение...")
                .AddButton(new ToastButton()
                    .SetTextBoxId("tbReply")
                    .SetContent("Ответить")
                    .AddArgument("userId", user.Id)
                    .AddArgument("conversationId", conversationId.ToString())
                    .AddArgument("action", "reply")
                    .SetImageUri(new Uri("ms-appx:///Assets/" + (IsDarkTheme() ? "reply_light.png" : "reply_dark.png")))
                )
                .AddButton(new ToastButton()
                    .SetContent("Заглушить на час")
                    .AddArgument("action", "dnd")
                    .AddArgument("parameters", user.Id)
                    .SetImageUri(new Uri("ms-appx:///Assets/" + (IsDarkTheme() ? "dnd_light.png" : "dnd_dark.png"))))
                .Show();
        }

        private async Task<string> SaveImage(byte[] image)
        {
            try
            {
                if (image == null)
                {
                    return "ms-appx:///Assets/" + (IsDarkTheme() ? "user_white.png" : "user.png");
                }
                var folder = ApplicationData.Current.LocalFolder;
                var files = await folder.GetFilesAsync();
                var file = files.ToList().FirstOrDefault(f => f.Name == "notification.png") ??
                           await folder.CreateFileAsync("notification.png");
                await FileIO.WriteBytesAsync(file, image);
                return file.Path;
            }
            catch (Exception e)
            {
                return "ms-appx:///Assets/" + (IsDarkTheme() ? "user_white.png" : "user.png");
            }
        }

        private bool IsDarkTheme()
        {
            return Application.Current.RequestedTheme == ApplicationTheme.Dark;
        }
    }

    public struct Triple<TX, TY, TZ>
    {
        public TX First;
        public TY Second;
        public TZ Third;

        public Triple(TZ third, TY second, TX first)
        {
            Third = third;
            Second = second;
            First = first;
        }
    }
}