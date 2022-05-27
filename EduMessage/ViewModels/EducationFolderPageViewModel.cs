using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class EducationFolderPageViewModel
    {
        [Property] private List<Speciality> _specialities;
        [PropertyCallMethod(nameof(Navigate))]        
        [Property] private Speciality _speciality;

        public async Task Initialize()
        {
            try
            {
                var response = (await (App.Address + "Login/Specialities")
                    .SendRequestAsync<string>(null, HttpRequestType.Get))
                    .DeserializeJson<List<Speciality>>();

                Specialities = response;
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

                
            }
        }

        private void Navigate()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedeEvent(Speciality));
        }
    }
}
