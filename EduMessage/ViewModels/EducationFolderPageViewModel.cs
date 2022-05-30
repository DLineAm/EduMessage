using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EduMessage.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class EducationFolderPageViewModel
    {
        [Property] private List<dynamic> _educationFolders;
        [PropertyCallMethod(nameof(Navigate))]        
        [Property] private dynamic _educationFolder;

        public async Task Initialize(object item)
        {
            try
            {
                var speciality = item as Speciality;
                if (speciality == null)
                {
                    var response = (await (App.Address + "Login/Specialities")
                            .SendRequestAsync<string>(null, HttpRequestType.Get))
                        .DeserializeJson<List<Speciality>>()
                        .Cast<dynamic>()
                        .ToList();

                    EducationFolders = response;
                    return;
                }
               
                var courses = (await (App.Address + $"Education/Courses.IdSpeciality={speciality.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<MainCourse>>()
                    .Cast<dynamic>()
                    .ToList();

                EducationFolders = courses;
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

                
            }
        }

        private void Navigate()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedeEvent(EducationFolder));
        }
    }
}
