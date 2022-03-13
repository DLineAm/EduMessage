﻿using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class SchoolChoosePageViewModel
    {
        [Property] private List<School> _schools;
        [Property] private List<EducationForm> _educationForms;
        [Property] private List<Speciality> _spetialities;
        [Property] private List<Group> _groups;
        [Property] private List<Role> _roles;
        [Property] private School _school;
        [Property] private EducationForm _educationForm;
        [Property] private Speciality _speciality;
        [Property] private Group _group;
        [Property] private City _city;
        [Property] private string _errorText;
        [PropertyCallMethod(nameof(ChangeVisibility))]
        [Property] private Role _role;
        [Property] private bool _isLoginEnabled = true;
        [Property] private Visibility _studentInputVisibility = Visibility.Visible;

        [Command(CanExecuteMethod = nameof(CanConfirm))]
        private async void Confirm()
        {
            try
            {
                await SetLoaderVisibility(Visibility.Visible);
                SetSpecialityFromGroup();

                if (Role.Id == 1 && Speciality == null)
                {
                    await SetLoaderVisibility(Visibility.Collapsed);
                    throw new Exception("Не удалось загрузить дополнительные данные");
                }

                App.Account.UserBuilder
                    .AddObject(Group)
                    .AddObject(Role)
                    .AddObject(EducationForm)
                    .AddObject(School);

                var result = await App.Account.Register(App.Account.UserBuilder);

                ErrorText = result ? "Ok" : "ne ok";
            }
            catch (Exception e)
            {
                ErrorText = "Произошла ошибка соединения с сервером: " + e.Message;
            }
            finally
            {
                await SetLoaderVisibility(Visibility.Collapsed);
            }
        }

        private void ChangeVisibility()
        {
            StudentInputVisibility = Role.Title == "Студент" ? Visibility.Visible : Visibility.Collapsed;
        }

        public async void LoadData()
        {
            try
            {
                var schools = (await (App.Address + "Home/Schools")
                    .PostBoolAsync(City))
                    .DeserializeJson<List<School>>();

                Schools = schools;


                var groups = (await (App.Address + "Home/Groups")
                    .GetStringAsync())
                    .DeserializeJson<List<Group>>();

                Groups = groups;

                var specialities = (await (App.Address + "Home/Specialities")
                    .GetStringAsync())
                    .DeserializeJson<List<Speciality>>();

                Spetialities = specialities;

                var forms = (await (App.Address + "Home/EducationForms")
                    .GetStringAsync())
                    .DeserializeJson<List<EducationForm>>();

                EducationForms = forms;

                var roles = (await (App.Address + "Home/Roles")
                    .GetStringAsync())
                    .DeserializeJson<List<Role>>();

                Roles = roles;
            }
            catch (Exception e)
            {
                ErrorText = "При соединении с сервером произошла ошибка: " + e.Message;
            }
        }

        private async void SetSpecialityFromGroup()
        {
            if (Group == null || Speciality != null && Speciality.Id == Group.IdSpeciality)
            {
                return;
            }

            try
            {
                var result = (await (App.Address + $"Home/Specialities.idGroup={Group.Id}")
                    .GetStringAsync())
                    .DeserializeJson<Speciality>();

                Speciality = result;
            }
            catch (Exception e)
            {

                ErrorText = "При соединении с сервером произошла ошибка: " + e.InnerException.Message;
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

        [CommandInvalidate(nameof(School), nameof(Group), nameof(EducationForm), nameof(Role), nameof(IsLoginEnabled))]
        private bool CanConfirm()
        {
            if (Role != null)
            {
                if (Role.Id == 1)
                {
                    return School != null &&
                           Group != null &&
                           EducationForm != null &&
                           Role != null &&
                           IsLoginEnabled;
                }

                return School != null &&
                       Role != null &&
                       IsLoginEnabled;
            }

            return false;

        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoBack()
        {
            new Navigator().Navigate(typeof(PersonalInfoAddPage),null ,new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight }, FrameType.LoginFrame);
        }

        [Command(CanExecuteMethod = nameof(IsButtonsEnabled))]
        private void GoToLogin()
        {
            new Navigator().Navigate(typeof(BaseLoginPage), null, new SlideNavigationTransitionInfo { Effect = SlideNavigationTransitionEffect.FromRight });
        }
    }
}