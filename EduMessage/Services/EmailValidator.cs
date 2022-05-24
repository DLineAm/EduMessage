using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EduMessage.Services
{
    public class EmailValidator : IValidator
    {
        public ValidationResponse Validate(string value)
        {
            try
            {
                new MailAddress(value);

                var response = ((App.Address + $"Login/Validate.email={value}")
                    .SendRequestAsync<string>(null, HttpRequestType.Get).Result)
                    .DeserializeJson<bool>();

                if (!response)
                {
                    return ValidationResponse.ExistsResponse;
                }

                return ValidationResponse.OkResponse;
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (FormatException e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {
                return ValidationResponse.FormatResponse;
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {
                return ValidationResponse.ServerErrorResponse;
            }
        }
    }
}
