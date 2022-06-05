
using MvvmGen;

using SignalIRServerTest;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationMainPageViewModel
    {
        public void Initialize()
        {
            //App.SelectedSpeciallityChanged += App_SelectedSpeciallityChanged;
        }
        private void App_SelectedSpeciallityChanged(Speciality speciality)
        {
            //Неопознанная ошибка COMException
        }
    }

}
