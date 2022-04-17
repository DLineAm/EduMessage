using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace EduMessage.Utils
{
    public class AutoSuggestQueryParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var args = (AutoSuggestBoxQuerySubmittedEventArgs) value;

            return args.ChosenSuggestion;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}