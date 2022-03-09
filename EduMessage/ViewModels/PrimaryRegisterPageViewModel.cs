﻿using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Services;
using MvvmGen;
using System.Net.Mail;
using Windows.UI.Xaml;
using EduMessage.Pages;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class PrimaryRegisterPageViewModel
    {
        private IValidator _passwordValidator;
        private IValidator _loginValidator;

        [Property] private string _email;
        [Property] private string _login;
        [Property] private string _password;
        [Property] private string _passwordRepeat;
        [Property] private string _errorText;
        [Property] private bool _isLoginEnabled = true;

        public void SetValidator(IValidator validator)
        {
            if (validator is PasswordValidator)
            {
                _passwordValidator = validator;
            }
            else
            {
                _loginValidator = validator;
            }
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoBack()
        {
            new Navigator().Navigate(typeof(BaseLoginPage), null, new SlideNavigationTransitionInfo{ Effect = SlideNavigationTransitionEffect.FromRight}, FrameType.LoginFrame);
        }

        [CommandInvalidate(nameof(IsLoginEnabled))]
        private bool IsButtonsEnabled()
        {
            return IsLoginEnabled;
        }

        [Command(CanExecuteMethod = nameof(CanRegister))]
        private async void Register()
        {
            await SetLoaderVisibility(Visibility.Visible);
            ErrorText = string.Empty;
            if (Password != PasswordRepeat)
            {
                ErrorText = "Пароль должнен совпадать с повтором пароля";
                await SetLoaderVisibility(Visibility.Collapsed);
                return;
            }

            var passwordResponse = _passwordValidator.Validate(Password);
            if (!passwordResponse)
            {
                ErrorText = passwordResponse.Status switch
                {
                    ValidateStatusType.Length => "Пароль должен быть длиной от 6 до 16 символов",
                    ValidateStatusType.Format => "expr",
                    ValidateStatusType.Other => "Неизвестная ошибка",
                    _ => throw new ArgumentOutOfRangeException()
                };
                await SetLoaderVisibility(Visibility.Collapsed);
                return;
            }

            var loginResponse = _loginValidator.Validate(Login);
            if (!loginResponse)
            {
                ErrorText = loginResponse.Status switch
                {
                    ValidateStatusType.Length => "Логин должен быть длиной от 6 до 16 символов",
                    ValidateStatusType.Format => "???",
                    ValidateStatusType.Exists => "Логин уже существует в базе данных",
                    ValidateStatusType.Server => "Ошибка подключения к серверу, повторите попытку позже",
                    ValidateStatusType.Other => "Неизвестная ошибка",
                    _ => throw new ArgumentOutOfRangeException()
                };
                await SetLoaderVisibility(Visibility.Collapsed);
                return;
            }

            try
            {
                new MailAddress(Email);

                var responseString = await (App.Address + $"Home/Send.email={Email}")
                    .GetStringAsync();

                if (string.IsNullOrEmpty(responseString) ||
                    !bool.TryParse(responseString, out var response) ||
                    !response)
                {
                    ErrorText = "Ошибка подключения к серверу, повторите попытку позже";
                    await SetLoaderVisibility(Visibility.Collapsed);
                    return;
                }

                App.Account.UserBuilder
                    .AddString("Login", Login)
                    .AddString("Password", Password);

                new Navigator().Navigate(typeof(EmailConfirmingPage)
                    , Email
                    , new SlideNavigationTransitionInfo {Effect = SlideNavigationTransitionEffect.FromLeft}
                    , FrameType.LoginFrame);
            }
            catch (FormatException e)
            {
                ErrorText = "Неверный формат почты";
            }
            catch (Exception e)
            {

                await new ContentDialog
                    {
                        Title = "Ошибка",
                        Content = "При отправке почты произошла ошибка: " + e.Message,
                        PrimaryButtonText = "Ok"
                    }
                    .ShowAsync();
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

        [CommandInvalidate(nameof(Email), nameof(Login), nameof(Password), nameof(PasswordRepeat), nameof(IsLoginEnabled))]
        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Email) &&
                !string.IsNullOrWhiteSpace(Login) &&
                !string.IsNullOrWhiteSpace(Password) &&
                !string.IsNullOrWhiteSpace(PasswordRepeat) && 
                IsLoginEnabled;
        }
    }
}