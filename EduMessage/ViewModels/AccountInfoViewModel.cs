using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class AccountInfoViewModel
    {
        [Property] private User _user;
        [Property] private string _fullName;
        [Property] private object _profilePicture;
        [Property] private Visibility _studentInfoVisibility;
        [Property] private Visibility _otherUserInputVisibility = Visibility.Visible;
        [Property] private Visibility _backButtonVisibility = Visibility.Collapsed;
        [Property] private string _password;
        [Property] private string _repeatPassword;
        //[Property] private string _errorText;
        [Property] private bool _isInfoBarOpen;
        [Property] private string _infoBarMessage;

        public async Task Initialize()
        {
            User = App.Account.GetUser();
            await BaseInitialize();
        }

        private void ChangeInfoBarMessage(string message = null)
        {
            InfoBarMessage = message;
            IsInfoBarOpen = message != null;
        }

        public async Task Initialize(User user)
        {
            User = user;
            await BaseInitialize();
            OtherUserInputVisibility = Visibility.Collapsed;
            BackButtonVisibility = Visibility.Visible;
        }

        private async Task BaseInitialize()
        {
            FullName = User.LastName + " " + User.FirstName + " " + User.MiddleName;

            if (User.Image != null)
            {
                ProfilePicture = await User.Image.CreateBitmap(256);
            }

            StudentInfoVisibility = User.IdRole == 2 ? Visibility.Collapsed : Visibility.Visible;
        }

        private async Task<StorageFile> PickSingleImageAsync()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            var file = await picker.PickSingleFileAsync();

            return file;
        }

        [Command]
        private void InitializeAccountDeleteDialog()
        {
            ChangeInfoBarMessage();
        }

        [Command]
        private void Back()
        {
            new Navigator().GoBack(FrameType.MenuFrame);
        }

        [Command]
        private void Logout()
        {
            App.Account.UpdateToken(false);
            EventAggregator.Publish(new UserExitedEvent());
        }

        [Command]
        private async void DeleteAccount()
        {
            ChangeInfoBarMessage();
            if (string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(RepeatPassword))
            {
                ChangeInfoBarMessage("Все поля должны быть заполнены!");
                return;
            }

            if (Password != RepeatPassword)
            {
                ChangeInfoBarMessage("Пароли должны совпадать!");
                return;
            }

            var response = await (App.Address + $"User/id={App.Account.GetUser().Id}.password={Password}")
                    .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt());
            switch (response)
            {
                case "Exception":
                    ChangeInfoBarMessage("Не удалось удалить пользователя, повторите попытку позже");
                    break;
                case "Not found by password" or "Not found by id":
                    ChangeInfoBarMessage("Неверное имя пользователя и/или пароль");
                    break;
                default:
                    EventAggregator.Publish(new DialogResultChanged(true));
                    App.Account.UpdateToken(false);
                    EventAggregator.Publish(new UserExitedEvent());
                    break;
            }
        }

        [Command]
        private async void PickImage()
        {
            StorageFile pickedFile = await PickSingleImageAsync();

            if (pickedFile != null)
            {
                var imageBytes = (await FileIO.ReadBufferAsync(pickedFile)).ToArray();

                if (imageBytes.Length == 0)
                {
                    await new ContentDialog
                    {
                        Title = "Ошибка изменения изображения",
                        Content = "Размер изображения должен быть больше 0",
                        PrimaryButtonText = "Ok"
                    }.ShowAsync();

                    return;
                }

                var pair = new KeyValuePair<int, byte[]>(User.Id, imageBytes);

                var response = (await (App.Address + "User/UploadImage")
                    .SendRequestAsync(pair, HttpRequestType.Put,App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (response)
                {
                    User.Image = imageBytes;
                    App.Account.GetUser().Image = imageBytes;

                    ProfilePicture = await imageBytes.CreateBitmap();

                    EventAggregator.Publish(new AccountImageUploadedEvent(imageBytes));
                }
            }
        }
    }
}
