namespace EduMessage.Services
{
    public interface IValidator<T>
    {
        ValidationResponse Validate(T value);
    }
}