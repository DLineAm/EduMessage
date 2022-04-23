using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

namespace EduMessage.Services
{
    public class DialogNotificator : INotificator, IDialogNotificator
    {
        private List<ContentDialog> _dialogs = new();
        public async void Notificate(string title, object message)
        {
            await new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Ok"
            }.ShowAsync();
        }

        public void Notificate(string title, string message, string knownDialogName = "")
        {
            if (knownDialogName == "")
            {
                Notificate(title, message);
            }

            var findedDialog = _dialogs.FirstOrDefault(d => d.Name == knownDialogName);

            if (findedDialog == null)
            {
                throw new IndexOutOfRangeException(nameof(knownDialogName));
            }

            findedDialog.Title = title;
        }
    }

    public interface IDialogNotificator
    {
        void Notificate(string title, string message, string knownDialogName = "");
    }
}
