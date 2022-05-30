using System.Linq;
using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationMainPage : Page, IEventSubscriber<EducationPageBack>
    {
        private IEventAggregator _eventAggregator;
        public EducationMainPage()
        {
            this.InitializeComponent();
            var eventAggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            _eventAggregator = eventAggregator;
            eventAggregator.RegisterSubscriber(this);
            ViewModel = new EducationMainPageViewModel(eventAggregator);
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
            var item = (Crumb)args.Item;
            var index = args.Index;

            GoBackUntil(item, index);
        }

        private void GoBackUntil(Crumb item, int index)
        {
            if (index == 0)
            {
                ViewModel.Crumbs.Clear();
                ViewModel.Crumbs.Add(new Crumb {Title = "Главная"});
                ContentFrame.Navigate(typeof(EducationFolderPage));
                return;
            }

            GoBackTo(item, index);

            while (ViewModel.Crumbs.Count > index + 1)
            {
                ViewModel.Crumbs.RemoveAt(ViewModel.Crumbs.Count - 1);
            }
        }

        private void GoBackTo(Crumb item, int index)
        {
            if (!ContentFrame.CanGoBack)
            {
                return;
            }

            var stack = ContentFrame.BackStack.ToList();
            ContentFrame.Navigate(stack[index].SourcePageType, item.Data);
        }

        public void OnEvent(EducationPageBack eventData)
        {
            var crumbs = ViewModel.Crumbs;
            var index = crumbs.Count - 2;
            var crumb = crumbs[index];

            GoBackUntil(crumb, index);
        }
    }
}
