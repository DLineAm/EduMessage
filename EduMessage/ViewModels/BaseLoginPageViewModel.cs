using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Pages;
using EduMessage.Services;
using MvvmGen;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using MvvmGen.Events;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class BaseLoginPageViewModel
    {
        [Property] private string _login;
        [Property] private string _password;
        [Property] private string _errorText;
        [Property] private bool _isSaveLogin = true;
        [Property] private bool _isLoginEnabled = true;
        private readonly SynchronizationContext _context;

        public BaseLoginPageViewModel()
        {
            _context = SynchronizationContext.Current;

        }

        [Command(CanExecuteMethod = nameof(CanLogin))]
        private async Task TryLogin()
        {
            ErrorText = string.Empty;
            await SetLoaderVisibility(Visibility.Visible);

            var result = await App.Account.Login(Login, Password, _isSaveLogin);

            await SetLoaderVisibility(Visibility.Collapsed);

            if (result != string.Empty)
            {
                if (result.Contains("New device"))
                {
                    var email = result.Split('=')[1];
                    new Navigator().Navigate(typeof(EmailConfirmingPage), new KeyValuePair<string,bool>(email,true), new SlideNavigationTransitionInfo(), FrameType.LoginFrame);
                    return;
                }
                ErrorText = result;
                return;
            }

            new Navigator().Navigate(typeof(MainMenuPage), null, new DrillInNavigationTransitionInfo());
        }

        private async Task SetLoaderVisibility(Visibility visibility)
        {
            if (App.OpenedWindows == 1)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                    CoreDispatcherPriority.Normal, async () =>
                    {
                        IsLoginEnabled = visibility != Visibility.Visible;

                        if (visibility == Visibility.Visible)
                        {
                            await Task.Delay(1);
                        }
                    });

                EventAggregator.Publish(new BaseLoaderVisibilityChangedEvent(visibility));
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
                new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromLeft },
                FrameType.LoginFrame);
        }



        [CommandInvalidate(nameof(Login), nameof(Password), nameof(IsLoginEnabled))]
        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password) && IsLoginEnabled;
        }
    }
}