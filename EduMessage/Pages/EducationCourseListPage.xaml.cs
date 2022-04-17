using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationCourseListPage : Page, IEventSubscriber<CourseDialogStartShowing>, IEventSubscriber<CourseAddedOrChangedEvent>
    {
        private bool _isPageLoaded;
        private bool _isCancelButtonPressed;
        private bool _isSuccessCourseAddOrChange;
        public EducationCourseListPage()
        {
            this.InitializeComponent();
            ViewModel = ControlContainer.Get().ResolveConstructor<EducationListPageViewModel>();

            ContentFrame.Navigate(typeof(ItemsPickPage));

            var page = ContentFrame.Content as ItemsPickPage;

            var eventAggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            page.Initialize(eventAggregator);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as Speciality;

            await ViewModel.Initialize(parameter);
            this.DataContext = ViewModel;

            App.EventAggregator.RegisterSubscriber(this);
        }

        public EducationListPageViewModel ViewModel { get; }

        public async void OnEvent(CourseDialogStartShowing eventData)
        {
            if (!_isPageLoaded)
            {
                return;
            }

            var parameter = eventData.IsAddMode;

            CourseAddDialog.Title = parameter ? "Добавление курса" : "Изменение курса";

            try
            {
                await CourseAddDialog.ShowAsync();
            }
            catch (Exception e)
            {

            }

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = true;
        }

        private void CourseAddDialog_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            args.Cancel = !_isCancelButtonPressed && !_isSuccessCourseAddOrChange;
        }

        private void CourseAddDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _isCancelButtonPressed = false;
        }

        private void CourseAddDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _isCancelButtonPressed = true;
        }

        public void OnEvent(CourseAddedOrChangedEvent eventData)
        {
            _isSuccessCourseAddOrChange = eventData.IsSuccess;
        }
    }
}
