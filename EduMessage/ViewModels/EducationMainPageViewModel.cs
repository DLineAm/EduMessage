using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Media.Animation;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationMainPageViewModel : IEventSubscriber<SelectedSpecialityChangedeEvent>
    {
        [Property]
        private ObservableCollection<Crumb> _crumbs = new()
        {
            new ("Главная", new EducationFolderPage())
        };

        public void Initialize()
        {
            //App.SelectedSpeciallityChanged += App_SelectedSpeciallityChanged;
        }

        public void OnEvent(SelectedSpecialityChangedeEvent eventData)
        {
            var speciality = eventData.Speciality;

            try
            {
                if (speciality == null)
                {
                    if (Crumbs.Count != 1)
                    {
                        Crumbs.Remove(Crumbs.LastOrDefault());
                    }
                    return;
                }
                var lastCrumb = Crumbs.LastOrDefault();
                var convertedTitle = speciality.Code + " " + speciality.Title;
                if (!lastCrumb.Equals(default(Crumb)) && lastCrumb.Title == convertedTitle)
                {
                    return;
                }

                Crumbs.Add(new Crumb(convertedTitle, null));
                new Navigator().Navigate(typeof(EducationCourseListPage), speciality, new DrillInNavigationTransitionInfo(), FrameType.EducationFrame);
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }
        }

        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
            
            
        }
    }

#pragma warning disable CS0659 // "Crumb" переопределяет Object.Equals(object o), но не переопределяет Object.GetHashCode().
}
