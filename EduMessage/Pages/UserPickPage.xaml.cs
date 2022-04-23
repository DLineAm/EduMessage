using EduMessage.Services;
using EduMessage.ViewModels;
using MvvmGen.Events;
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

            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
            ViewModel = new UserPickViewModel(aggregator, notificator);
            await ViewModel.Initialize();

            DataContext = ViewModel;
        }

        public UserPickViewModel ViewModel { get; private set; }
    }
}
