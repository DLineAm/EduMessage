using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;

using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(INotificator))]
    public partial class EmailConfirmingPageViewModel
    {
        [Property] private string _code;
        [Property] private string _errorText;
        [Property] private bool _isLoginEnabled = true;
        public string Email; 

        [Command(CanExecuteMethod = nameof(CanConfirm))]
        private async void Confirm()
        {
            try
            {
                await SetLoaderVisibility(Visibility.Visible);
                var responseString = await (App.Address + $"Login/Validate.email={Email}.emailCode={Code}")
                    .SendRequestAsync("", HttpRequestType.Get);

                if (string.IsNullOrEmpty(responseString))
                {
                    ErrorText = "Не удалось подключиться к серверу, повторите попытку позже";
                    await SetLoaderVisibility(Visibility.Collapsed);
                    return;
                }

                if (bool.TryParse(responseString, out var response) &&
                    !response)
                {
                    ErrorText = "Неверный код подтверждения";
                    await SetLoaderVisibility(Visibility.Collapsed);
                    return;
                }

                App.Account.UserBuilder.AddString("Email", Email);
                new Navigator().Navigate(typeof(PersonalInfoAddPage),
                    null
                    , new SlideNavigationTransitionInfo {Effect = SlideNavigationTransitionEffect.FromLeft}
                    , FrameType.LoginFrame);
            }
            catch (System.Exception e)
            {
                var message = "При отправке запроса произошла ошибка: " + e.Message;
                Notificator.Notificate("Ошибка", message);
            }
            finally
            {
                await SetLoaderVisibility(Visibility.Collapsed);
            }
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

        [CommandInvalidate(nameof(Code), nameof(IsLoginEnabled))]
        private bool CanConfirm()
        {
            return !string.IsNullOrEmpty(Code) & IsLoginEnabled;
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoBack()
        {
            new Navigator().GoBack(new SlideNavigationTransitionInfo {Effect = SlideNavigationTransitionEffect.FromRight}, FrameType.LoginFrame);
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoToLogin()
        {
            new Navigator().Navigate(typeof(BaseLoginPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }

        [CommandInvalidate(nameof(IsLoginEnabled))]
        private bool IsButtonsEnabled()
        {
            return IsLoginEnabled;
        }

    }
}
