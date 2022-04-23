
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using Windows.UI.Core;
using Windows.UI.Xaml;

namespace EduMessage.Services
{
    public class Account
    {
        private readonly INotificator _notificator;
        public User User { get; private set; }

        public string Jwt { get; private set; }

        public IUserBuilder UserBuilder { get; }

        public Account(IUserBuilder userBuilder, INotificator notificator)
        {
            _notificator = notificator;
            UserBuilder = userBuilder;
        }

        public async Task<bool> TryLoadToken(IChat chat)
        {
            try
            {
                var settings = Settings.Get();

                if (!settings.Values.TryGetValue("Jwt", out var jwt))
                {
                    return false;
                }

                Jwt = jwt as string;

                var user = (await (App.Address + "Login/GetUser.ByToken")
                    .SendRequestAsync("", HttpRequestType.Get, Jwt))
                    .DeserializeJson<User>();

                if (user == null)
                {
                    return false;
                }

                chat.Initialize(App.Address + "Chat", Jwt);
                chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", ReceiveMessage);
                await chat.OpenConnection();

                User = user;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async void ReceiveMessage(List<MessageAttachment> message, User user)
        {
            if (Window.Current.CoreWindow.ActivationMode is CoreWindowActivationMode.None or CoreWindowActivationMode.ActivatedInForeground)
            {
                return;
            }
            try
            {
                var pair = new KeyValuePair<List<MessageAttachment>, User>(message, user);
                _notificator.Notificate("Message", pair);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        public void UpdateToken(bool isAddMode)
        {
            var settings = Settings.Get();
            if (isAddMode)
            {
                settings.Values["Jwt"] = Jwt;
            }
            else
            {
                settings.Values.Remove("Jwt");
            }
        }

        public async Task<string> Login(string loginEmail, string password, bool isSaveLogin)
        {
            try
            {
                var pair = (await (App.Address + $"Login/Users.login={loginEmail}.password={password}")
                    .SendRequestAsync("", HttpRequestType.Get))
                    .DeserializeJson<KeyValuePair<User, string>>();
                var user = pair.Key;
                var token = pair.Value;
                if (user == null)
                {
                    return "Неверное имя пользователя или пароль";
                }

                Jwt = token;


                UpdateToken(isSaveLogin);

                User = user;

                var chat = ControlContainer.Get().Resolve<IChat>();
                chat.Initialize(App.Address + "Chat", Jwt);
                chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", ReceiveMessage);
                await chat.OpenConnection();

                return string.Empty;
            }
            catch (Exception e)
            {
                return "При соединении с сервером произошла ошибка: " + e.Message;
            }

        }

        public async Task<bool> Register(IUserBuilder builder)
        {
            if (!builder.UserInvalidate())
            {
                throw new InvalidOperationException("Все поля пользователя должны быть заполнены");
            }

            var user = builder.Build();
            User = user;

            var result = (await (App.Address + "Login/Register")
                .SendRequestAsync(User, HttpRequestType.Post))
                .DeserializeJson<KeyValuePair<int, string>>();

            var savedUserId = result.Key;
            var token = result.Value;

            if (!string.IsNullOrEmpty(token))
            {
                Jwt = token;

                UpdateToken(true);
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
