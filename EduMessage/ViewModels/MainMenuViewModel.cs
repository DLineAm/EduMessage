using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using System;

using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class MainMenuViewModel : IEventSubscriber<AccountImageUploadedEvent>, IEventSubscriber<LoaderVisibilityChanged>, IEventSubscriber<ColorChangedEvent>
    {
        [Property] private string _accountName;
        [Property] private object _accountImage;
        [Property] private Visibility _loaderVisibility = Visibility.Collapsed;
        [Property] private string _loaderText;
        [Property] private Color _loaderColor = App.ColorManager.GetAccentColor();

        public void Initialize()
        {
            AccountName = App.Account.User.FirstName + " " + App.Account.User.LastName;
            var imageBytes = App.Account.User.Image;
            ChangeProfilePicture(imageBytes);
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
                var bitmap = await imageBytes.CreateBitmap();
                AccountImage = bitmap;
            }
        }
    }
}
