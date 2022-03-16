using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

namespace EduMessage.Services
{
    public class DialogNotificator : INotificator
    {
        public async void Notificate(string title, string message)
        {
            await new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Ok"
            }.ShowAsync();
        }
    }
}
