using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationFolderPage : Page
    {
        public EducationFolderPage()
        {
            this.InitializeComponent();
            ViewModel = ControlContainer.Get()
                .ResolveConstructor<EducationFolderPageViewModel>();
            this.DataContext = ViewModel;
        }

        public EducationFolderPageViewModel ViewModel{ get;}

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var item = e.Parameter;

            await ViewModel?.Initialize(item);
        }

        private async void Page_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
        }
    }
}
