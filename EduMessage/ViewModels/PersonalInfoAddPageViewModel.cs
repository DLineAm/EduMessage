using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class PersonalInfoAddPageViewModel
    {
        [Property] private string _personName;
        [Property] private int _phone;
        [Property] private City _city;
        [Property] private List<City> _cities;
        [Property] private string _errorText;
        [Property] private bool _isLoginEnabled = true;
        private IValidator _personValidator;

        public async void LoadData()
        {
            try
            {
                await SetLoaderVisibility(Visibility.Visible);
                var result = (await (App.Address + "Home/Cities")
                        .GetStringAsync())
                    .DeserializeJson<List<City>>();

                Cities = result;
            }
            catch (Exception e)
            {

            }
            finally
            {
                await SetLoaderVisibility(Visibility.Collapsed);
            }

        }

        public void SetValidator(IValidator validator)
        {
            _personValidator = validator;
        }

        [Command(CanExecuteMethod = nameof(CanConfirm))]
        private async void Confirm()
        {
            try
            {
                await SetLoaderVisibility(Visibility.Visible);
                var result = _personValidator.Validate(PersonName);
                if (!result)
                {
                    ErrorText = result.Status switch
                    {
                        ValidateStatusType.Length => "Неверная длина поля ФИО",
                        ValidateStatusType.Format => "Неверный формат ФИО",
                        ValidateStatusType.Other => "Произошла неизвестная ошибка",
                    };
                    await SetLoaderVisibility(Visibility.Collapsed);
                    return;
                }

                var phoneValidateResult = (await (App.Address + $"Home/Validate.phone={Phone}")
                        .GetStringAsync())
                    .DeserializeJson<bool>();

                if (!phoneValidateResult)
                {
                    ErrorText = "Введенный номер телефона уже привязан к существующему аккаунту";
                    await SetLoaderVisibility(Visibility.Collapsed);
                    return;
                }

                App.Account.UserBuilder
                    .AddString("person", PersonName)
                    .AddString("phone", Phone.ToString())
                    .AddObject(City);


                new Navigator().Navigate(typeof(SchoolChoosePage)
                    , City
                    , new SlideNavigationTransitionInfo {Effect = SlideNavigationTransitionEffect.FromLeft}
                    , FrameType.LoginFrame);
            }
            catch (Exception e)
            {
                await new ContentDialog
                {
                    Title = "Ошибка",
                    Content = "При подключении к серверу произошла ошибка: " + e.Message,
                    PrimaryButtonText = "Ok"
                }.ShowAsync();
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

        [CommandInvalidate(nameof(IsLoginEnabled))]
        private bool IsButtonsEnabled()
        {
            return IsLoginEnabled;
        }

        [CommandInvalidate(nameof(City), nameof(PersonName), nameof(Phone), nameof(IsLoginEnabled))]
        private bool CanConfirm()
        {
            return City != null && 
                   !string.IsNullOrEmpty(PersonName) &&
                   Phone.ToString().Length == 11 &&
                   IsLoginEnabled;
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoBack()
        {
            new Navigator().Navigate(typeof(PrimaryRegistrationPage),null,new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }, FrameType.LoginFrame);
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoToLogin()
        {
            new Navigator().Navigate(typeof(BaseLoginPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}
