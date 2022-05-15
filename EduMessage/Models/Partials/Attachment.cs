using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using EduMessage.Annotations;
using EduMessage.Services;
using Microsoft.Toolkit.Uwp.Helpers;

namespace SignalIRServerTest.Models
{
    public partial class Attachment : INotifyPropertyChanged
    {
        [JsonIgnore]
        private object _imagePath;

        [JsonIgnore]
        public object ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public async Task SplitAndGetImage()
        {
            ImagePath = await SplitAndGetImage(Title, Data);
        }

        private async Task<object> SplitAndGetImage(string type, byte[] data)
        {
            var list = type.Split(".");

            if (list.Length == 1)
            {
                return await GetImage(type, data);
            }

            return await GetImage("." + list.Last(), data);
        }

        public async Task OpenFile()
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile;
            if (await storageFolder.FileExistsAsync(Title))
            {
                storageFile = await storageFolder.GetFileAsync(Title);
                await FileIO.WriteTextAsync(storageFile, "");
            }
            else
            {
                storageFile = await storageFolder.CreateFileAsync(Title);
            }

            await FileIO.WriteBytesAsync(storageFile, Data);
            await Launcher.LaunchFileAsync(storageFile);
        }

        public int ConvertFileType(string type) => type switch
        {
            ".pdf" => 1,
            ".txt" => 2,
            ".png" or ".jpg" or ".jpeg" => 3,
            ".docx" or ".doc" => 4,
            _ => 5
        };

        //private async Task<object> SplitAndGetImage(string type, byte[] data)
        //{
        //    var list = type.Split(".");

        //    if (list.Length == 1)
        //    {
        //        return await GetImage(type, data);
        //    }

        //    return await GetImage("." + list.Last(), data);
        //}

        private async Task<object> GetImage(string type, byte[] data) => type switch
        {
            ".docx" => "ms-appx:///Assets/word.png",
            ".pdf" => "ms-appx:///Assets/pdf.png",
            ".txt" => "ms-appx:///Assets/txt.png",
            ".png" or ".jpg" or ".jpeg" => await data.CreateBitmap(48),
            _ => "ms-appx:///Assets/file.png"
        };

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}