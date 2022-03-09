using Newtonsoft.Json;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

namespace EduMessage.Services
{
    public class Account
    {
        public  User User { get; private set; }

        public  string Jwt { get; private set; }

        public IUserBuilder UserBuilder { get; }

        public Account(IUserBuilder userBuilder)
        {
            UserBuilder = userBuilder;
        }



        public async Task<bool> TryLoadTokenFromJson()
        {
            try
            {
                var settings = Settings.Get();

                if (!settings.Values.TryGetValue("Jwt", out var jwt))
                {
                    return false;
                }

                Jwt = jwt as string;

                var user = (await (App.Address + "Home/GetUser.ByToken")
                    .GetStringAsync(Jwt))
                    .DeserializeJson<User>();

                if (user == null)
                {
                    return false;
                }

                User = user;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public void SaveTokenToJson()
        {
            var settings = Settings.Get();
            settings.Values["Jwt"] = Jwt;
        }

        public  async Task<string> Login(string loginEmail, string password)
        {
            try
            {
                var pair = (await (App.Address + $"Home/Users.login={loginEmail}.password={password}")
                    .GetStringAsync())
                    .DeserializeJson<KeyValuePair<User, string>>();
                var user = pair.Key;
                var token = pair.Value;
                if (user == null)
                {
                    return "Неверное имя пользователя или пароль";
                }

                Jwt = token;

                SaveTokenToJson();
                User = user;
                return string.Empty;
            }
            catch (Exception e)
            {
                return "При соединении с сервером произошла ошибка: " + e.Message;
            }

        }

        public  async Task<bool> Register(IUserBuilder builder)
        {
            if (!builder.UserInvalidate())
            {
                throw new InvalidOperationException("Все поля пользователя должны быть заполнены");
            }

            var user = builder.Build();
            User = user;
            var result = (await (App.Address + "Home/Register").PostBoolAsync(User)).DeserializeJson<KeyValuePair<int, string>>();

            var savedUserId = result.Key;
            var token = result.Value;

            if (!string.IsNullOrEmpty(token))
            {
                Jwt = token;

                SaveTokenToJson();
            }

            if (savedUserId == -1)
            {
                return false;
            }
            User.Id = savedUserId;
            return true;
        }

        
    }
}
