using System;
using EduMessage.ViewModels;

using SignalIRServerTest.Models;

using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using EduMessage.Services;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class TreeChangePage : Page
    {
        private IEventAggregator _aggregator;
        public TreeChangePage()
        {
            this.InitializeComponent();
            _aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not User user) return;

            ViewModel = ControlContainer.Get().ResolveConstructor<TreeChangePageViewModel>();
            await ViewModel.Initialize();
            DataContext = ViewModel;
        }

        public TreeChangePageViewModel ViewModel { get; private set; }

        private async void UIElement_OnDrop(object sender, DragEventArgs e)
        {
            if ((sender as Border)?.DataContext is not MainCourseTree mainCourse)
            {
                return;
            }
            if ( await e.DataView.GetDataAsync("CourseId") is int courseId &&
                 await e.DataView.GetDataAsync("MainCourseId") is int mainCourseId)
            {
                _aggregator.Publish(new UiElementDropCompletedEvent(courseId, mainCourseId, mainCourse.Id));
            }
            //var data = await e.Data.GetView().GetDataAsync("UiElement");
        }

        private void UIElement_OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Move;
        }

        private void UIElement_OnDragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.AllowedOperations = DataPackageOperation.Move;
            var context = ((Border) sender).DataContext as CourseTree;
            args.Data.SetData("CourseId", context.Id);
            args.Data.SetData("MainCourseId", context.MainCourseId);
            //args.Data.SetData("SpecialityCourseId", context.Id);
        }
    }

    public record UiElementDropCompletedEvent(int CourseId, int OldMainCourseId, int NewMainCourse);
}
