using System;
using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationFolderPage : Page, IEventSubscriber<DialogStatusChanged>
    {
        private bool _isContentDialogCompleted;
        public EducationFolderPage()
        {
            this.InitializeComponent();
            var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            aggregator.RegisterSubscriber(this);
            ViewModel = new EducationFolderPageViewModel(notificator, aggregator);
            this.DataContext = ViewModel;
        }

        public EducationFolderPageViewModel ViewModel{ get;}

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var item = e.Parameter;

            var validator = ControlContainer.Get().Resolve<IValidator<string[]>>();

            await ViewModel.Initialize(item, validator);
        }

        private async void Page_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
        }

        private async void ChangeRecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            _isContentDialogCompleted = false;
            while (!_isContentDialogCompleted)
            {
                await RecordEditDialog.ShowAsync();
            }
        }

        private void RecordEditDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _isContentDialogCompleted = true;
            RecordEditDialog.Hide();
        }

        public void OnEvent(DialogStatusChanged eventData)
        {
            var isSuccess = eventData.IsSuccess;
            _isContentDialogCompleted = isSuccess;
            if (isSuccess)
            {
                RecordEditDialog.Hide();
            }
        }
    }
}
