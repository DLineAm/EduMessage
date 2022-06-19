using System;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class AddOrChangeTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value == null ? "Добавить" : "Изменить";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}