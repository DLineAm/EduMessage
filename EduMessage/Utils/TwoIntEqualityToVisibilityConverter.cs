using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class TwoIntEqualityToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not int id || parameter is not string otherIdString || !int.TryParse(otherIdString, out int otherId))
            {
                return Visibility.Collapsed;
            }

            return id == otherId ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}