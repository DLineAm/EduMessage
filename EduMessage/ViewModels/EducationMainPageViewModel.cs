using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Windows.UI.Xaml.Media.Animation;
using EduMessage.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationMainPageViewModel : IEventSubscriber<SelectedSpecialityChangedeEvent>
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

        public void OnEvent(SelectedSpecialityChangedeEvent eventData)
        {
            var parameter = eventData.Parameter;

            if (parameter == null)
            {
                return;
            }

            var crumb = new Crumb {Data = parameter};

            if (parameter is int id)
            {
                crumb.Title = (id == 0 ? "Создание " : "Изменение ") + "темы";
                Crumbs.Add(crumb);
                new Navigator().Navigate(typeof(ThemeConstructorPage), id, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                return;
            }

            var title = parameter.Title;
            if (parameter is Speciality speciality)
            {
                title = speciality.Code + " " + speciality.Title;
            }

            crumb.Title = title;
            Crumbs.Add(crumb);
            if (parameter is MainCourse)
            {
                new Navigator().Navigate(typeof(EducationCourseListPage), parameter, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
                return;
            }
            new Navigator().Navigate(typeof(EducationFolderPage), parameter, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);

//            try
//            {
//                if (parameter == null)
//                {
//                    if (Crumbs.Count != 1)
//                    {
//                        Crumbs.Remove(Crumbs.LastOrDefault());
//                    }
//                    return;
//                }
//                var lastCrumb = Crumbs.LastOrDefault();
//                var convertedTitle = parameter.Code + " " + parameter.Title;
//                if (!lastCrumb.Equals(default(Crumb)) && lastCrumb.Title == convertedTitle)
//                {
//                    return;
//                }

//                Crumbs.Add(new Crumb(convertedTitle, null));
//                new Navigator().Navigate(typeof(EducationCourseListPage), parameter, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
//            }
//#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
//            catch (Exception e)
//#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
//            {

//            }
        }

        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
            
            
        }
    }

#pragma warning disable CS0659 // "Crumb" переопределяет Object.Equals(object o), но не переопределяет Object.GetHashCode().
}
