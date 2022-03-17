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
                    .SendRequestAsync("", HttpRequestType.Get).Result)
                    .DeserializeJson<bool>();

                if (!response)
                {
                    return ValidationResponse.ExistsResponse;
                }

                return ValidationResponse.OkResponse;
            }
            catch (FormatException e)
            {
                return ValidationResponse.FormatResponse;
            }
            catch (Exception e)
            {
                return ValidationResponse.ServerErrorResponse;
            }
        }
    }
}
