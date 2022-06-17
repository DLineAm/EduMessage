using Windows.UI.Xaml;
using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WebApplication1;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TestConstructorPage : Page
    {
        public TestConstructorPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            ViewModel = ControlContainer.Get().ResolveConstructor<TestConstructorPageViewModel>();
            await ViewModel.Initialize();
            DataContext = ViewModel;
        }

        public TestConstructorPageViewModel ViewModel { get; set; }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var context = ((AppBarButton) sender).DataContext;
            if (context is not TestVariant testVariant)
            {
                return;
            }

            var page = testVariant.IdTestPageNavigation;
            page.TestVariants.Remove(testVariant);
            testVariant.IdTestPageNavigation = null;
        }

        private void TeachingTipButton_OnClick(object sender, RoutedEventArgs e)
        {
            TeachingTip.IsOpen = true;
        }
    }
}
