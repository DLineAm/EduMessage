namespace EduMessage.Services
{
    public interface IDialogNotificator
    {
        void Notificate(string title, string message, string knownDialogName = "");
    }
}