using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using System;

using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ThemeConstructorPage : Page, IEventSubscriber<FeatureAdded>, IEventSubscriber<CourseTaskDialogOpedRequested>, IEventSubscriber<DialogStatusChanged>
    {
        private bool _contentDialogCompleted;

        public ThemeConstructorPage()
        {
            this.InitializeComponent();
            
        }

        public ThemeConstructorPageViewModel ViewModel { get; private set; }
        public void OnEvent(FeatureAdded eventData)
        {
            var position = eventData.Length;
            DescriptionBox.Document.Selection.StartPosition = position;
            DescriptionBox.Focus(FocusState.Keyboard);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not (int mainCourseId, FormattedCourse course))
            {
                return;
            }

            var container = ControlContainer.Get();
            var collection = container.ResolveConstructor<FeatureCollection>();
            var aggregator = container.Resolve<IEventAggregator>();
            var notificator = container.Resolve<INotificator>("Dialog");
            aggregator.RegisterSubscriber(this);
            ViewModel = new ThemeConstructorPageViewModel(notificator,aggregator, collection);
            if (course.Course == null)
            {
                ViewModel.Initialize(mainCourseId);
            }
            else
            {
                await ViewModel.Initialize(mainCourseId, course);
            }
            this.DataContext = ViewModel;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            DescriptionBox.Focus(FocusState.Keyboard);
            var bulletButton = (ToggleButton)sender;
            if (bulletButton.IsChecked is false)
            {
                DescriptionBox.Document.Selection.ParagraphFormat.ListType = MarkerType.None;
                return;
            }
            var makerType = MarkerType.Arabic;
            DescriptionBox.Document.Selection.ParagraphFormat.ListStart = 1;
            DescriptionBox.Document.Selection.ParagraphFormat.ListType = makerType;
        }

        private void DescriptionBox_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            e.Handled = true;
        }

        public async void OnEvent(CourseTaskDialogOpedRequested eventData)
        {
            _contentDialogCompleted = false;
            try
            {
                while (!_contentDialogCompleted)
                {
                    await TaskDialog.ShowAsync();
                }
            }
            catch (Exception e)
            {

            }
        }

        public void OnEvent(DialogStatusChanged eventData)
        {
            var success = eventData.IsSuccess;
            _contentDialogCompleted = success;
            if (_contentDialogCompleted)
            {
                TaskDialog.Hide();
            }
        }

        private void TaskDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _contentDialogCompleted = true;
            TaskDialog.Hide();
        }
    }
}
