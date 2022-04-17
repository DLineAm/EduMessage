using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserPickPage : Page
    {
        public UserPickPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel = ControlContainer.Get().ResolveConstructor<UserPickViewModel>();
            await ViewModel.Initialize();

            DataContext = ViewModel;
        }

        public UserPickViewModel ViewModel { get; private set; }
    }
}
