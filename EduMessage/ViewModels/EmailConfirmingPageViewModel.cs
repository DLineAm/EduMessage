using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(INotificator))]
    [Inject(typeof(IEventAggregator))]
    public partial class EmailConfirmingPageViewModel
    {
        [Property] private string _code;
        [Property] private string _errorText;
        [Property] private bool _isLoginEnabled = true;
        [Property] private string _title;

        private string _email;
        private bool _isLoadedFromLogin;

        public void Initialize(string email, bool isLoadedFromLogin)
        {
            _email = email;
            _isLoadedFromLogin = isLoadedFromLogin;

            if (isLoadedFromLogin)
            {
                Title = "Вход с нового устройства";
                return;
            }

            Title = "Регистрация";
        }

        [Command(CanExecuteMethod = nameof(CanConfirm))]
        private async void Confirm()
        {
            try
            {
                await SetLoaderVisibility(Visibility.Visible);
                var responseString = await (App.Address + $"Login/ValidateCode.email={_email}.emailCode={Code}")
                    .SendRequestAsync<string>(null, HttpRequestType.Get);

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

                if (_isLoadedFromLogin)
                {
                    var machineName = Environment.MachineName;
                    var device = new Device
                    {
                        CreateDate = DateTime.Now,
                        IdUser = App.Account.GetUser().Id,
                        Name = machineName,
                        SerialNumber = App.Account.GetMachineId()
                    };

                    var deviceResponse = (await (App.Address + "Device")
                            .SendRequestAsync(device, HttpRequestType.Post))
                        .DeserializeJson<bool>();
                    if (!deviceResponse)
                    {
                        ErrorText = "Не удалось проверить подлинность устройства, повторите попытку позже";
                    }
                    App.Account.UpdateToken(true);
                    new Navigator().Navigate(typeof(MainMenuPage), null, new DrillInNavigationTransitionInfo());
                    return;
                }

                App.Account.UserBuilder.AddString("email", _email);
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
            EventAggregator.Publish(new BaseLoaderVisibilityChangedEvent(visibility));

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
