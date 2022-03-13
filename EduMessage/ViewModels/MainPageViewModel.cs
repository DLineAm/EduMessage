using EduMessage.Pages;
using EduMessage.Services;

using MvvmGen;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class MainPageViewModel
    {
        [Property] private object _selectedContent;

        public async void Initialize()
        {
            _selectedContent = await App.Account.TryLoadToken() ? new MainMenuPage() : new LoginPage();

        }
    }
}
