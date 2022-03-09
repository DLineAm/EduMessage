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
    public partial class MainMenuViewModel
    {
        [Property] private string _accountName;

        public void Initialize()
        {
            AccountName = App.Account.User.Login;
        }
    }
}
