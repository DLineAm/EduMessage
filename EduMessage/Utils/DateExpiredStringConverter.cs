using System;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class DateExpiredStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not DateTime dateTime)
            {
                return null;
            }

            return dateTime > DateTime.Now ? string.Empty : "(Пропущен срок сдачи)";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}