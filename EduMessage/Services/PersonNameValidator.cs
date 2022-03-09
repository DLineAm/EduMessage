using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduMessage.Services
{
    public class PersonNameValidator : IValidator
    {
        public ValidationResponse Validate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return ValidationResponse.LengthResponse;
            }

            var list = value.Split(' ');

            if (list.Length != 3)
            {
                return ValidationResponse.FormatResponse;
            }

           return ValidationResponse.OkResponse;
        }
    }
}
