
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

        [ThreadStatic] private static User _user;

        [ThreadStatic] private static string _jwt;

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

                _jwt = jwt as string;

                var user = (await (App.Address + "Login/GetUser.ByToken")
                    .SendRequestAsync("", HttpRequestType.Get, _jwt))
                    .DeserializeJson<User>();

                if (user == null)
                {
                    return false;
                }

                chat.Initialize(App.Address + "Chat", GetJwt());
                chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", ReceiveMessage);
                await chat.OpenConnection();

                _user = user;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public User GetUser()
        {
            return _user;
        }

        private void ReceiveMessage(List<MessageAttachment> message, User user)
        {
            if (Window.Current == null ||
                Window.Current.CoreWindow.ActivationMode is CoreWindowActivationMode.None or CoreWindowActivationMode.ActivatedInForeground ||
                App.IsDoNotDisturbEnabled)
            {
                return;
            }
            try
            {
                var pair = new KeyValuePair<List<MessageAttachment>, User>(message, user);
                _notificator.Notificate("FormattedMessageContent", pair);
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
                settings.Values["Jwt"] = GetJwt();
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
                var (user, token) = (await (App.Address + $"Login/Users.login={loginEmail}.password={password}")
                        .SendRequestAsync("", HttpRequestType.Get))
                    .DeserializeJson<KeyValuePair<User, string>>();
                if (user == null)
                {
                    return "Неверное имя пользователя или пароль";
                }

                _jwt = token;

                UpdateToken(isSaveLogin);

                var chat = ControlContainer.Get().Resolve<IChat>();
                chat.Initialize(App.Address + "Chat", GetJwt());
                chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", ReceiveMessage);
                await chat.OpenConnection();

                _user = user;

                return string.Empty;
            }
            catch (Exception e)
            {
                return "При соединении с сервером произошла ошибка: " + e.Message;
            }

        }

        public string GetJwt()
        {
            return _jwt;
        }

        public async Task<bool> Register(IUserBuilder builder)
        {
            if (!builder.UserInvalidate())
            {
                throw new InvalidOperationException("Все поля пользователя должны быть заполнены");
            }

            var user = builder.Build();
            _user = user;

            var (savedUserId, token) = (await (App.Address + "Login/Register")
                    .SendRequestAsync(_user, HttpRequestType.Post))
                .DeserializeJson<KeyValuePair<int, string>>();

            if (!string.IsNullOrEmpty(token))
            {
                _jwt = token;

                UpdateToken(true);
            }

            if (savedUserId == -1)
            {
                return false;
            }
            _user.Id = savedUserId;
            return true;
        }


    }
}
