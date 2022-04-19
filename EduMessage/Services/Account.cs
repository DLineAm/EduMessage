
using Microsoft.Toolkit.Uwp.Notifications;

using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace EduMessage.Services
{
    public class Account
    {
        public User User { get; private set; }

        public string Jwt { get; private set; }

        public IUserBuilder UserBuilder { get; }

        public Account(IUserBuilder userBuilder)
        {
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
                chat.SetOnMethod<string, User>("ReceiveForMe", async (m, u) =>
                {
                    if (Window.Current.CoreWindow.ActivationMode is CoreWindowActivationMode.None or CoreWindowActivationMode.ActivatedInForeground)
                    {
                        return;
                    }
                    new ToastContentBuilder()
                        .AddText(u.FirstName + " " + u.LastName, hintMaxLines: 1)
                        .AddAppLogoOverride(new Uri(await SaveImage(u.Image)), ToastGenericAppLogoCrop.Circle)
                        .AddText(m)
                        .AddInputTextBox("tbReply", "Напишите сообщение...")
                        .AddButton(new ToastButton()
                            .SetTextBoxId("tbReply")
                            .SetContent("Ответить")
                            .AddArgument("userId", u.Id)
                            .AddArgument("action", "reply")
                            .SetImageUri(new Uri("ms-appx:///Assets/" + (IsDarkTheme() ? "reply_light.png" : "reply_dark.png")))
                        )
                        .AddButton(new ToastButton()
                            .SetContent("Заглушить на час")
                            .AddArgument("action", "dnd")
                            .AddArgument("parameters", u.Id)
                            .SetImageUri(new Uri("ms-appx:///Assets/" + (IsDarkTheme() ? "dnd_light.png" : "dnd_dark.png"))))
                        .Show();
                });
                await chat.OpenConnection();

                User = user;

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private async Task<string> SaveImage(byte[] image)
        {
            if (image == null)
            {
                
                return "ms-appx:///Assets/" + (IsDarkTheme() ?  "user_white.png" : "user.png");
            }
            var folder = ApplicationData.Current.LocalFolder;
            var files = await folder.GetFilesAsync();
            var file = files.ToList().FirstOrDefault(f => f.Name == "notification.png") ??
                       await folder.CreateFileAsync("notification.png");
            await FileIO.WriteBytesAsync(file, image);
            return file.Path;
        }

        private bool IsDarkTheme()
        {
            return Application.Current.RequestedTheme == ApplicationTheme.Dark;
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
