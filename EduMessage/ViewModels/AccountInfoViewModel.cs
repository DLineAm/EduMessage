﻿using EduMessage.Services;

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

        public async void Initialize()
        {
            User = App.Account.User;
            FullName = User.LastName + " " + User.FirstName + " " + User.MiddleName;

            if (User.Image != null)
            {
                ProfilePicture = await User.Image.CreateBitmap();
            }
            
        }

        [Command]
        private async void PickImage()
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile pickedFile = await picker.PickSingleFileAsync();

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
