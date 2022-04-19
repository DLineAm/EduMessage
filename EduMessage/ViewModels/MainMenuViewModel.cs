using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class MainMenuViewModel : IEventSubscriber<AccountImageUploadedEvent>, IEventSubscriber<LoaderVisibilityChanged>, IEventSubscriber<ColorChangedEvent>
    {
        [Property] private string _accountName;
        [Property] private object _accountImage;
        [Property] private Visibility _loaderVisibility = Visibility.Collapsed;
        [Property] private string _loaderText;
        [Property] private Color _loaderColor = App.ColorManager.GetAccentColor();

        public async Task Initialize()
        {
            AccountName = App.Account.User.FirstName + " " + App.Account.User.LastName;
            var imageBytes = App.Account.User.Image;
            ChangeProfilePicture(imageBytes);

            var response = (await (App.Address + "Message/All")
                    .SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                .DeserializeJson<List<UserConversation>>();

            EventAggregator.Publish(new ConversationGot(response));
        }

        public void OnEvent(AccountImageUploadedEvent eventData)
        {
            ChangeProfilePicture(eventData.ImageBytes);
        }

        public void OnEvent(LoaderVisibilityChanged eventData)
        {
            LoaderVisibility = eventData.LoaderVisibility;
            LoaderText = eventData.LoaderText;
        }

        public async void OnEvent(ColorChangedEvent eventData)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () =>
                {
                    LoaderColor = eventData.Color;
                });
            
        }

        private async void ChangeProfilePicture(byte[] imageBytes) 
        {
            if (imageBytes != null)
            {
                var bitmap = await imageBytes.CreateBitmap(36);
                AccountImage = bitmap;
            }
        }

        [Command]
        private void Exit()
        {
            App.Account.UpdateToken(false);
            EventAggregator.Publish(new UserExitedEvent());
        }
    }

    public record UserExitedEvent();

    public record ConversationGot(List<UserConversation> Conversations);
}
