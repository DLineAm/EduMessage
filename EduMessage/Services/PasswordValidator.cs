namespace EduMessage.Services
{
    public class PasswordValidator : IValidator<string>
    {
        public ValidationResponse Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 6 || value.Length > 16)
            {
                return ValidationResponse.LengthResponse;
            }

            return ValidationResponse.OkResponse;
        }
    }
}