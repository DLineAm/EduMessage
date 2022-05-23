using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class MainPageViewModel : IEventSubscriber<UserExitedEvent>
    {
        [Property] private object _selectedContent;

        public async void Initialize()
        {
            var chat = ControlContainer.Get().Resolve<IChat>();
            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (App.IsAlreadyLaunched)
                {
                    SelectedContent = new LoginPage();
                    return;
                }
                var result = await App.Account.TryLoadToken(chat);

                if (result)
                {
                    SelectedContent = new MainMenuPage();
                }
                else
                {
                    SelectedContent = new LoginPage();
                }
                App.IsAlreadyLaunched = true;

            });
        }

        public void OnEvent(UserExitedEvent eventData)
        {
            var chat = ControlContainer.Get().Resolve<IChat>();
            chat.CloseConnection();
            SelectedContent = new LoginPage();
        }
    }
}
