using System;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class MarkTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not byte mark)
            {
                return null;
            }

            return mark switch
            {
                2 => value + " (неудовлетворительно)",
                3 => value + " (удовлетворительно)",
                4 => value + " (хорошо)",
                5 => value + " (отлично)",
                _ => null,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}