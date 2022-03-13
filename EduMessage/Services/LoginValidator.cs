﻿namespace EduMessage.Services
{
    public class LoginValidator : IValidator
    {
        public ValidationResponse Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length is < 6 or > 16)
            {
                return ValidationResponse.LengthResponse;
            }

            return ValidationResponse.OkResponse;
        }
    }
}