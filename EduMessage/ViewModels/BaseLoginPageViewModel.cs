using System;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Pages;
using EduMessage.Services;
using MvvmGen;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Xaml;
using System.Threading.Tasks;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class BaseLoginPageViewModel
    {
        [Property] private string _login;
        [Property] private string _password;
        [Property] private bool _isSavelogin = true;
        [Property] private string _errorText;
        [Property] private bool _isLoginEnabled = true;

        [Command(CanExecuteMethod = nameof(CanLogin))]
        private async Task TryLogin()
        {
            ErrorText = string.Empty;
            await SetLoaderVisibility(Visibility.Visible);
           
            var result = await App.Account.Login(Login, Password);
            if (result != string.Empty)
            {
                ErrorText = result;
                await SetLoaderVisibility(Visibility.Collapsed);
                return;
            }

            //ErrorText = "Ok";
            await SetLoaderVisibility(Visibility.Collapsed);

            new Navigator().Navigate(typeof(MainMenuPage), null, new DrillInNavigationTransitionInfo());
        }

        private async Task SetLoaderVisibility(Visibility visibility)
        {
            IsLoginEnabled = visibility != Visibility.Visible;
            App.InvokeLoaderVisibilityChanged(visibility);

            if (visibility == Visibility.Visible)
            {
                await Task.Delay(1);
            }
        }

        [CommandInvalidate(nameof(IsLoginEnabled))]
        private bool IsButtonsEnabled()
        {
            return IsLoginEnabled;
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void Register()
        {
            new Navigator().Navigate(typeof(PrimaryRegistrationPage),
                null,
                new SlideNavigationTransitionInfo{ Effect = SlideNavigationTransitionEffect.FromLeft },
                FrameType.LoginFrame);
        }

        

        [CommandInvalidate(nameof(Login), nameof(Password), nameof(IsLoginEnabled))]
        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password) && IsLoginEnabled;
        }
    }
}