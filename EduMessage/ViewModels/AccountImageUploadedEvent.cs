namespace EduMessage.ViewModels
{
    public class AccountImageUploadedEvent
    {
        public byte[] ImageBytes { get; set; }

        public AccountImageUploadedEvent(byte[] imageBytes)
        {
            this.ImageBytes = imageBytes;
        }
    }
}