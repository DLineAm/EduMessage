using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                    .SendRequestAsync("", HttpRequestType.Get))
                    .DeserializeJson<List<Speciality>>();

                Specialities = response;
            }
            catch (Exception e)
            {

                
            }
        }

        private void Navigate()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedeEvent(Speciality));
        }
    }

    public record SelectedSpecialityChangedeEvent(Speciality Speciality);
}
