using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;

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
    public partial class EducationMainPageViewModel
    {
        [Property]
        private ObservableCollection<Crumb> _crumbs = new()
        {
            new ("Главная", new EducationFolderPage())
        };

        public void Initialize()
        {
            App.SelectedSpeciallityChanged += App_SelectedSpeciallityChanged;
        }

        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
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
                if (lastCrumb != null && lastCrumb.Title == convertedTitle)
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
    }

    public class Crumb
    {
        public string Title { get; set; }
        public object Data { get; set; }

        public Crumb(string title, object data)
        {
            Title = title;
            Data = data;
        }
    }
}
