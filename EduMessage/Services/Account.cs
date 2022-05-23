
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Xaml;
using SignalIRServerTest;

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
                var isDeviceNotExists = (await (App.Address + "Device/Validate")
                        .SendRequestAsync(GetMachineId(), HttpRequestType.Get))
                    .DeserializeJson<bool>();


                var (user, token) = (await (App.Address + $"Login/Users.login={loginEmail}.password={password}")
                        .SendRequestAsync("", HttpRequestType.Get))
                    .DeserializeJson<KeyValuePair<User, string>>();
                if (user == null)
                {
                    return "Неверное имя пользователя или пароль";
                }

                _user = user;

                _jwt = token;

                if (isDeviceNotExists)
                {
                    var responseString = (await (App.Address + $"Login/Send.email={user.Email}")
                        .SendRequestAsync("", HttpRequestType.Get))
                        .DeserializeJson<bool>();
                    if (!responseString)
                    {
                        return "Ошибка подключения к серверу, повторите попытку позже";
                    }
                    return $"New device={user.Email}";
                }

                UpdateToken(isSaveLogin);

                var chat = ControlContainer.Get().Resolve<IChat>();
                chat.Initialize(App.Address + "Chat", GetJwt());
                chat.SetOnMethod<List<MessageAttachment>, User>("ReceiveForMe", ReceiveMessage);
                await chat.OpenConnection();


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

                var machineName = System.Environment.MachineName;

                var device = new Device
                {
                    CreateDate = DateTime.Now,
                    IdUser = savedUserId,
                    Name = machineName,
                    SerialNumber = GetMachineId()
                };

                var response = (await (App.Address + "Device")
                        .SendRequestAsync(device, HttpRequestType.Post))
                    .DeserializeJson<bool>();
            }

            if (savedUserId == -1)
            {
                return false;
            }
            _user.Id = savedUserId;
            return true;
        }

        public byte[] GetMachineId()
        {
            SystemIdentificationInfo systemId = SystemIdentification.GetSystemIdForPublisher();
            byte[] buffer = systemId.Id.ToArray();
            return buffer;
        }
    }
}
