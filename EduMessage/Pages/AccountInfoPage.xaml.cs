using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using SignalIRServerTest.Models;

using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class AccountInfoPage : Page, IEventSubscriber<DialogResultChanged>
    {
        private bool _contentDialogCompleted;

        public AccountInfoPage()
        {
            this.InitializeComponent();
            var aggregator = ControlContainer.Get()
                .Resolve<IEventAggregator>();
            aggregator.RegisterSubscriber(this);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as User;
            ViewModel = ControlContainer.Get().ResolveConstructor<AccountInfoViewModel>();

            if (parameter != null)
            {
                await ViewModel.Initialize(parameter);
                this.DataContext = ViewModel;
                return;
            }

            await ViewModel.Initialize();
            this.DataContext = ViewModel;
        }

        public AccountInfoViewModel ViewModel { get; private set; }

        private async void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            _contentDialogCompleted = false;
            while (!_contentDialogCompleted)
            {
                await ContentDialog.ShowAsync();
            }
        }

        private void ContentDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _contentDialogCompleted = true;
        }

        private void ContentDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        public void OnEvent(DialogResultChanged eventData)
        {
            _contentDialogCompleted = eventData.Result;
            if (eventData.Result)
            {
                ContentDialog.Hide();
            }
        }
    }
}
