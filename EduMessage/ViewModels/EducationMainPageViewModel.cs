using EduMessage.Models;
using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.ObjectModel;

using Windows.UI.Xaml.Media.Animation;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationMainPageViewModel : IEventSubscriber<SelectedSpecialityChangedEvent>
    {
        [Property]
        private ObservableCollection<Crumb> _crumbs = new()
        {
            new Crumb{Title = "Главная"}
        };

        public void Initialize()
        {
            //App.SelectedSpeciallityChanged += App_SelectedSpeciallityChanged;
        }

        public void OnEvent(SelectedSpecialityChangedEvent eventData)
        {

            var parameter = eventData.Parameter;

            if (parameter == null)
            {
                return;
            }

            var crumb = new Crumb {Data = parameter};
            var navigator = new Navigator();
            switch (parameter)
            {
                case User user:
                    crumb.Title = "Изменение иерархии";
                    navigator.Navigate(typeof(TreeChangePage), user, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                    break;
                case int id:
                    crumb.Title = (id == 0 ? "Создание " : "Изменение ") + "темы";
                    //Crumbs.Add(crumb);
                    navigator.Navigate(typeof(ThemeConstructorPage), id, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                    break;
                case Speciality speciality:
                    crumb.Title = speciality.Code + " " + speciality.Title;
                    navigator.Navigate(typeof(EducationFolderPage), parameter, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                    break;
                case MainCourse mainCourse:
                    crumb.Title = mainCourse.Title;
                    navigator.Navigate(typeof(EducationCourseListPage), parameter, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                    break;
            }
            Crumbs.Add(crumb);
        }

        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
        }
    }

}
