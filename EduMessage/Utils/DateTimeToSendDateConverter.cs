using System;
using System.Globalization;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class DateTimeToSendDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = (DateTime)value;

            var culture = new CultureInfo("ru-RU");
            var day = culture.DateTimeFormat.GetDayName(date.DayOfWeek);
            return $"{string.Concat(day[0].ToString().ToUpper(), day.Substring(1))}, {date:dd.MM}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}