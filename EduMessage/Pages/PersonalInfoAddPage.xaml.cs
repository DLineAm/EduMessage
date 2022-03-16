using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class PersonalInfoAddPage : Page
    {
        public PersonalInfoAddPage()
        {
            this.InitializeComponent();
            ViewModel = App.Container.ResolveConstructor<PersonalInfoAddPageViewModel>();
            var validator = App.Container.Resolve<IValidator>("person");
            ViewModel.SetValidator(validator);
            ViewModel.LoadData();
            this.DataContext = ViewModel;
        }

        public PersonalInfoAddPageViewModel ViewModel { get; }
    }
}
