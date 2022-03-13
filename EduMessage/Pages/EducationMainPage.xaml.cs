using EduMessage.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationMainPage : Page
    {
        public EducationMainPage()
        {
            this.InitializeComponent();
            ViewModel = new EducationMainPageViewModel();
            this.DataContext = ViewModel;
            ContentFrame.Navigate(typeof(EducationFolderPage));
        }
       
        public EducationMainPageViewModel ViewModel { get; }

        private void Page_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ViewModel.Initialize();
        }

        private void BreadcrumbBar_ItemClicked(Microsoft.UI.Xaml.Controls.BreadcrumbBar sender, Microsoft.UI.Xaml.Controls.BreadcrumbBarItemClickedEventArgs args)
        {
            App.InvokeSelectedSpecialityChanged(null);
            ContentFrame.Navigate(typeof(EducationFolderPage));           
        }
    }
}
