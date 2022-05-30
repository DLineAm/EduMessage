using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml.Controls;
using MvvmGen.Events;

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
            var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            ViewModel = new PersonalInfoAddPageViewModel(aggregator, notificator);
            var validator = ControlContainer.Get().Resolve<IValidator<string>>("person");
            ViewModel.SetValidator(validator);
            ViewModel.LoadData();
            this.DataContext = ViewModel;
        }

        public PersonalInfoAddPageViewModel ViewModel { get; }
    }
}
