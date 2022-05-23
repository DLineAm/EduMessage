using EduMessage.Services;
using EduMessage.ViewModels;

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
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EmailConfirmingPage : Page
    {
        public EmailConfirmingPage()
        {
            this.InitializeComponent();
            var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            ViewModel = new EmailConfirmingPageViewModel(aggregator, notificator);
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is KeyValuePair<string, bool>(var email, var isLoadedFromLogin))
            {
                ViewModel.Initialize(email, isLoadedFromLogin);
            }
            
        }

        public EmailConfirmingPageViewModel ViewModel { get; }
    }
}
