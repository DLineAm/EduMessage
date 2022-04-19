using System;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace EduMessage.Utils
{
    public class ByteToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not byte[] bytes)
            {
                return null;
            }

            var source = new BitmapImage {DecodePixelHeight = 48, DecodePixelWidth = 48};

            if (parameter is not null && int.TryParse(parameter.ToString(), out var decodeHeightWidth))
            {
                source.DecodePixelWidth = decodeHeightWidth;
                source.DecodePixelHeight = decodeHeightWidth;
            }

            using var stream = new InMemoryRandomAccessStream();
            stream.WriteAsync(bytes.AsBuffer()).GetResults();
            stream.Seek(0);
            source.SetSource(stream);

            return source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}