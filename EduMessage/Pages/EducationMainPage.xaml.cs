using System;
using System.Collections.ObjectModel;
using System.Linq;
using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Models;
using SignalIRServerTest;
using SignalIRServerTest.Models;
using WebApplication1;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationMainPage : Page, IEventSubscriber<EducationPageBack>, IEventSubscriber<SelectedSpecialityChangedEvent>
    {
        private IEventAggregator _eventAggregator;
        public EducationMainPage()
        {
            this.InitializeComponent();
            var eventAggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            _eventAggregator = eventAggregator;
            eventAggregator.RegisterSubscriber(this);
            ViewModel = new EducationMainPageViewModel();
            this.DataContext = ViewModel;
            ContentFrame.Navigate(typeof(EducationFolderPage));
        }

        public EducationMainPageViewModel ViewModel { get; }

        public ObservableCollection<Crumb> Crumbs { get; set; } = new()
        {
            new Crumb { Title = "Главная" }
        };

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
            var backStack = ContentFrame.BackStack;

            var lastPage = Crumbs.LastOrDefault();

            if (lastPage.Data is (int, FormattedCourse))
            {
                var themeConstructorPage = ContentFrame.Content as ThemeConstructorPage;
                themeConstructorPage.ViewModel.SaveData = false;
            }


            if (index == 0)
            {
                Crumbs.Clear();
                Crumbs.Add(new Crumb { Title = "Главная" });
                backStack.Clear();
                ContentFrame.Navigate(typeof(EducationFolderPage));
                return;
            }

            GoBackTo(item, index);


            while (Crumbs.Count > index + 1)
            {
                Crumbs.RemoveAt(Crumbs.Count - 1);
                backStack.RemoveAt(ContentFrame.BackStack.Count - 1);
            }
        }

        private void GoBackTo(Crumb item, int index)
        {
            if (!ContentFrame.CanGoBack)
            {
                return;
            }

            var stack = ContentFrame.BackStack.ToList();
            var pageFromStack = stack.FirstOrDefault(p => p.Parameter?.GetType() == item.Data.GetType());
            if (pageFromStack == null)
            {
                return;
            }
            ContentFrame.Navigate(pageFromStack.SourcePageType, item.Data);
        }

        public void OnEvent(EducationPageBack eventData)
        {
            var crumbs = Crumbs;
            var index = crumbs.Count - 2;
            var crumb = crumbs[index];

            GoBackUntil(crumb, index);
        }

        public void OnEvent(SelectedSpecialityChangedEvent eventData)
        {
            var parameter = eventData.Parameter;

            try
            {
                if (parameter == null)
                {
                    return;
                }
            }
            catch
            {
                // ignored
            }

            var crumb = new Crumb { Data = parameter };
            var navigator = new Navigator();
            switch (parameter)
            {
                case User user:
                    crumb.Title = "Изменение иерархии";
                    navigator.Navigate(typeof(TreeChangePage), user, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                    break;
                case (int userId, Course course):
                    crumb.Title = "Выполнение тестирования";
                    ContentFrame.Navigate(typeof(TestPassPage), (userId, course),
                        new DrillInNavigationTransitionInfo());
                    break;
                case (int id, FormattedCourse formattedCourse):
                    //crumb.Data = parameter;
                    crumb.Title = (formattedCourse.Course == null ? "Создание " : "Изменение ") + "темы";
                    ContentFrame.Navigate(typeof(ThemeConstructorPage), (id, formattedCourse),
                        new DrillInNavigationTransitionInfo());
                    break;
                case (int id, MainCourse mainCourse):
                    crumb.Data = mainCourse;
                    crumb.Title = "Журнал оценок";
                    ContentFrame.Navigate(typeof(JournalPage), (id, mainCourse),
                        new DrillInNavigationTransitionInfo());
                    break;
                case Speciality speciality:
                    crumb.Title = speciality.Code + " " + speciality.Title;
                    ContentFrame.Navigate(typeof(EducationFolderPage), parameter,
                        new DrillInNavigationTransitionInfo());
                    break;
                case MainCourse mainCourse:
                    crumb.Title = mainCourse.Title;
                    ContentFrame.Navigate(typeof(EducationCourseListPage), parameter,
                        new DrillInNavigationTransitionInfo());
                    break;
                case int courseId:
                    crumb.Title = "Присланные работы";
                    ContentFrame.Navigate(typeof(UserTaskListPage), courseId,
                        new DrillInNavigationTransitionInfo());
                    break;
                case Course course:
                    crumb.Title = (course.IdTestFrame == null ? "Добавление" : "Изменение") + " тестов"; 
                    ContentFrame.Navigate(typeof(TestConstructorPage), course,
                        new DrillInNavigationTransitionInfo());
                    break;
            }

            Crumbs.Add(crumb);
        }
    }
}
