namespace EduMessage.Services
{
    public class ValidationResponse
    {
        public static ValidationResponse OkResponse { get; } = new() { Status = ValidateStatusType.Ok };
        public static ValidationResponse LengthResponse { get; } = new() { Status = ValidateStatusType.Length };
        public static ValidationResponse FormatResponse { get; } = new() { Status = ValidateStatusType.Format };
        public static ValidationResponse ExistsResponse { get; } = new() { Status = ValidateStatusType.Exists };
        public static ValidationResponse ServerErrorResponse { get; } = new() { Status = ValidateStatusType.Server };
        public static ValidationResponse OtherErrorResponse { get; } = new() { Status = ValidateStatusType.Other };

        public ValidateStatusType Status { get; private set; }

        public static implicit operator bool(ValidationResponse validationResponse)
        {
            return validationResponse.Status == ValidateStatusType.Ok;
        }

    }
}