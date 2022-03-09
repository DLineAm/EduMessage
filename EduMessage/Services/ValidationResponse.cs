namespace EduMessage.Services
{
    public class ValidationResponse
    {
        public static ValidationResponse OkResponse = new() { Status = ValidateStatusType.Ok };
        public static ValidationResponse LengthResponse = new() { Status = ValidateStatusType.Length };
        public static ValidationResponse FormatResponse = new() { Status = ValidateStatusType.Format };
        public static ValidationResponse ExistsResponse = new() { Status = ValidateStatusType.Exists };
        public static ValidationResponse ServerErrorResponse = new() { Status = ValidateStatusType.Server };
        public static ValidationResponse OtherErrorResponse = new() { Status = ValidateStatusType.Other };

        public ValidateStatusType Status { get; private set; }

        public static implicit operator bool(ValidationResponse validationResponse)
        {
            return validationResponse.Status == ValidateStatusType.Ok;
        }

    }
}