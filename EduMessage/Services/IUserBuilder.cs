using System.Collections.Generic;
using SignalIRServerTest;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public interface IUserBuilder
    {
        IUserBuilder AddString(string propName, string value);
        IUserBuilder AddObject(object value);
        bool UserInvalidate();
        KeyValuePair<User, string> Build();
    }
}