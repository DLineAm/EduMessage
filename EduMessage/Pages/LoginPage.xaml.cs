using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

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
