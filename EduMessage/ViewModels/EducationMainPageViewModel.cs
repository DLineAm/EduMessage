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
            catch (Exception e)
            {

            }
        }

        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
            
            
        }
    }

    public struct Crumb
    {
        public string Title { get; set; }
        public object Data { get; set; }

        public Crumb(string title, object data)
        {
            Title = title;
            Data = data;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if (obj == null || obj is not Crumb crumb)
            {
                return false;
            }

            // TODO: write your implementation of Equals() here
            if (crumb.Title == Title && crumb.Data == Data)
            {
                return true;
            }
            return false;
        }
    }
}
