using System;
using Windows.UI.Xaml.Data;
using SignalIRServerTest;

namespace EduMessage.Utils
{
    public class TypeToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return "";
            }
            var type = value.GetType();

            return type == typeof(Speciality) ? "специальность " : "дисциплину ";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}