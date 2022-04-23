using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EduMessage.Services;
using EduMessage.ViewModels;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PrimaryRegistrationPage : Page
    {
        public PrimaryRegistrationPage()
        {
            this.InitializeComponent();
            if (ViewModel == null)
            {
                var passwordValidator = ControlContainer.Get().Resolve<IValidator>("password");
                var personValidator = ControlContainer.Get().Resolve<IValidator>("login");
                var emailValidator = ControlContainer.Get().Resolve<IValidator>("email");
                var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
                var eventAggregator = ControlContainer.Get().Resolve<IEventAggregator>();

                ViewModel = new PrimaryRegisterPageViewModel(eventAggregator, notificator);

                ViewModel.SetValidator(passwordValidator);
                ViewModel.SetValidator(personValidator);
                ViewModel.SetValidator(emailValidator);

                this.DataContext = ViewModel;
            }
            
        }

        public PrimaryRegisterPageViewModel ViewModel { get; private set; }
    }
}
