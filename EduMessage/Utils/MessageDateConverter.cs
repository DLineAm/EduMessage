using System;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class MessageDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;

            var difference = DateTime.Now - date;

            if (difference.Days > 0)
            {
                return date.ToString("g");
            }

            return date.ToString("t");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}