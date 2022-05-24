using System.Collections.Generic;
using SignalIRServerTest;
using SignalIRServerTest.Models;

namespace EduMessage.Services
{
    public class UserBuilder : IUserBuilder
    {
        private static User _user;
        private static string _password;

        public UserBuilder()
        {
            _user = new User();
        }

        public IUserBuilder AddString(string propName, string value)
        {
            switch (propName.ToLower())
            {
                case "login":
                    _user.Login = value;
                    break;
                case "email":
                    _user.Email = value;
                    break;
                case "password":
                    _password = value;
                    break;
                case "person":
                    var names = value.Split(' ');
                    _user.LastName = names[0];
                    _user.FirstName = names[1];
                    _user.MiddleName = names[2];
                    break;
                case "phone":
                    var phone = long.Parse(value);
                    _user.Phone = phone;
                    break;
            }
            return this;
        }

        public IUserBuilder AddObject(object value)
        {
            switch (value)
            {
                case City city:
                    _user.IdCityNavigation = city;
                    _user.IdCity = city.Id;
                    break;
                case Group group:
                    _user.IdGroup = group.Id;
                    _user.IdGroupNavigation = group;
                    break;
                case EducationForm form:
                    _user.IdEducationForm = form.Id;
                    _user.IdEducationFormNavigation = form;
                    break;
                case School school:
                    _user.IdSchool = school.Id;
                    _user.IdSchoolNavigation = school;
                    break;
                case Role role:
                    _user.IdRole = role.Id;
                    _user.IdRoleNavigation = role;
                    break;
            }

            return this;
        }

        public bool UserInvalidate()
        {
            if (_user.IdRoleNavigation != null)
            {
                if (string.IsNullOrWhiteSpace(_user.Login) ||
                    string.IsNullOrWhiteSpace(_user.Email) ||
                    string.IsNullOrWhiteSpace(_password) ||
                    _user.IdRoleNavigation == null ||
                    _user.IdSchoolNavigation == null)
                {
                    return false;
                }
                if (_user.IdRoleNavigation.Id == 1)
                {
                    return _user.IdGroupNavigation != null &&
                           _user.IdEducationFormNavigation != null;
                }

                return true;
            }

            else
            {
                return false;
            }
        }

        public KeyValuePair<User, string> Build()
        {
            return new KeyValuePair<User, string>(_user, _password);
        }
    }
}