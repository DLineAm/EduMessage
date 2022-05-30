using System;

namespace EduMessage.Services
{
    public class NullOrWhiteSpaceStringValidator : IValidator<string[]>
    {
        public ValidationResponse Validate(string[] value)
        {
            foreach (var str in value)
            {
                if (string.IsNullOrWhiteSpace(str))
                {
                    return ValidationResponse.LengthResponse;
                }
            }

            return ValidationResponse.OkResponse;
        }
    }
}