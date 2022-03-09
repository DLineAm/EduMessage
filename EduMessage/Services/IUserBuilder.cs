using SignalIRServerTest;

namespace EduMessage.Services
{
    public interface IUserBuilder
    {
        IUserBuilder AddString(string propName, string value);
        IUserBuilder AddObject(object value);
        bool UserInvalidate();
        User Build();
    }
}