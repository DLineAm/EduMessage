using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using System.Threading.Tasks;

using Windows.UI;
using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class LoginPageViewModel : IEventSubscriber<ColorChangedEvent>
    {
        [Property] private Visibility _loaderVisibility = Visibility.Collapsed;
        [Property] private Color _loaderColor = App.ColorManager.GetAccentColor();

        public void Initialize()
        {
            App.LoaderVisibiltyChanged += v => { LoaderVisibility = v; };
        }

        public void OnEvent(ColorChangedEvent eventData)
        {
            LoaderColor = eventData.Color;
            //Bindings.Update();
        }
    }
}
