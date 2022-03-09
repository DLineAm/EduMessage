namespace EduMessage.Services
{
    public interface IValidator
    {
        ValidationResponse Validate(string value);
    }
}