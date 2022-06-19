using EduMessage.Services;
using EduMessage.ViewModels;

using System;
using Windows.UI.Xaml;
using WebApplication1;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SignalIRServerTest;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestPassPage : Page
    {
        public TestPassPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not (int idUser, Course course))
            {
                return;
            }

            ViewModel = ControlContainer.Get().ResolveConstructor<TestPassPageViewModel>();
            ViewModel.IniTialize(idUser, course);
            DataContext = ViewModel;
        }

        public TestPassPageViewModel ViewModel { get; private set; }

        private async void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ResultContentDialog.ShowAsync();
        }
    }
}
