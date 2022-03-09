using System;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.ApplicationModel.Email;

namespace EduMessage.Services
{
    public class Email
    {
        public static AlternateView CreateAlternateView(string filePath)
        {
            LinkedResource res = new LinkedResource(filePath);
            res.ContentId = Guid.NewGuid().ToString();
            string htmlBody = @"<img src='cid:" + res.ContentId + @"'/>";
            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(res);
            return alternateView;
        }
        public void Send(string address, string htmlContent, params AlternateView[] images)
        {
            var from = new MailAddress("pochta23114@gmail.com", "EduMessage");
            var to = new MailAddress(address);
            var message = new MailMessage(from, to);

            

            if (images != null && images.Length != 0)
            {
                foreach (var item in images)
                {
                    message.AlternateViews.Add(item);
                }
            }

            message.Subject = "Подтверждение почты";

            message.Body = htmlContent;
            message.IsBodyHtml = true;

            var client = new SmtpClient("smtp.gmail.com", 587);
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("pochta23114@gmail.com", "pHPgeo1-D");
            client.EnableSsl = true;
            client.Send(message);
        }
    }
}