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
        [Property] private List<UserConversation> _conversationList;
        [Property] private Visibility _adminMenuItemsVisibility = Visibility.Collapsed;

        public async Task Initialize()
        {
            var user = App.Account.GetUser();
            AccountName = user.FirstName + " " + user.LastName;
            var imageBytes = user.Image;
            ChangeProfilePicture(imageBytes);
            AdminMenuItemsVisibility = user.IdRole == 3 ? Visibility.Visible : Visibility.Collapsed;
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
            if (imageBytes == null) return;
            var bitmap = await imageBytes.CreateBitmap(36);
            AccountImage = bitmap;
        }
    }
}
