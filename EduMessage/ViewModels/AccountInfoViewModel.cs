using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using Newtonsoft.Json.Converters;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SignalIRServerTest.Models;

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

        public async Task Initialize()
        {
            User = App.Account.User;
            await BaseInitialize();
        }

        public async Task Initialize(User user)
        {
            User = user;
            await BaseInitialize();
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
                    .SendRequestAsync(pair, HttpRequestType.Put,App.Account.Jwt))
                    .DeserializeJson<bool>();

                if (response)
                {
                    User.Image = imageBytes;
                    App.Account.User.Image = imageBytes;

                    ProfilePicture = await imageBytes.CreateBitmap();

                    EventAggregator.Publish(new AccountImageUploadedEvent(imageBytes));
                }
            }
        }
    }

}
