using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using EduMessage.ViewModels;
using Microsoft.Toolkit.Uwp.UI.Helpers;
using Microsoft.UI.Xaml.Controls;
using EduMessage.Services;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        
        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = ControlContainer.Get().ResolveConstructor<LoginPageViewModel>();
            ViewModel.Initialize();
            this.DataContext = ViewModel;

            ContentFrame.Navigate(typeof(BaseLoginPage), null, new DrillInNavigationTransitionInfo());

        }

        //TODO: Визибилиту лоадер, сохранение jwt токена

        public Visibility LoaderVisibility { get; set; }


        public LoginPageViewModel ViewModel { get; private set; }

    }
}
