using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace EduMessage.Utils
{
    public class ColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var brush = (SolidColorBrush) value;

            return brush.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}