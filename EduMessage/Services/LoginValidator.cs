namespace EduMessage.Services
{
    public class LoginValidator : IValidator
    {
        public ValidationResponse Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length is < 6 or > 16)
            {
                return ValidationResponse.LengthResponse;
            }

            try
            {
                var response = (App.Address + $"Login/Validate.login={value}")
                    .SendRequestAsync("", HttpRequestType.Get).Result
                    .DeserializeJson<bool>();

                if (!response)
                {
                    return ValidationResponse.ExistsResponse;
                }

                return ValidationResponse.OkResponse;

            }
            catch (System.Exception e)
            {
                return ValidationResponse.ServerErrorResponse;
            }
        }
    }
}