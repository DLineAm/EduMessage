using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class AccountInfoViewModel
    {
        [Property] private User _user;
        [Property] private string _fullName;

        public void Initialize()
        {
            User = App.Account.User;
            FullName = User.LastName + " " + User.FirstName + " " + User.MiddleName;
        }
    }
}
