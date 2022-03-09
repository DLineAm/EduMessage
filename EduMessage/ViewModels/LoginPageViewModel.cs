using EduMessage.Services;

using MvvmGen;

using System.Threading.Tasks;

using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class LoginPageViewModel
    {
        [Property] private Visibility _loaderVisibility = Visibility.Collapsed;

        public void Initialize()
        {
            App.LoaderVisibiltyChanged += v => { LoaderVisibility = v; };
        }
    }
}
