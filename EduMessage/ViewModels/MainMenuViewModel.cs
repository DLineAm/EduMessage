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
        [Property] private object _accountImage;

        public void Initialize()
        {
            AccountName = App.Account.User.FirstName + " " + App.Account.User.LastName;
            var imageBytes = App.Account.User.Image;
            AccountImage = imageBytes == null || imageBytes.Length == 0
                ? "ms-appx:///Assets/userPictureWhite.png"
                : imageBytes;
        }
    }
}
