using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class NotApprovedUsersPage : Page
    {
        public NotApprovedUsersPage()
        {
            this.InitializeComponent();
        }

        public NotApprovedUsersPageViewModel ViewModel { get; private set; }

        private async void NotApprovedUsersPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = ControlContainer.Get().ResolveConstructor<NotApprovedUsersPageViewModel>();
            await ViewModel.Initialize();
            this.DataContext = ViewModel;
        }
    }
}
