using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace EduMessage.Services
{
    public class TextBlockVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is true
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}