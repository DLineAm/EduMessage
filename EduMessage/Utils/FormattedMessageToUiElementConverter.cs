using System;
using Windows.UI.Xaml.Data;
using EduMessage.Resources;
using EduMessage.ViewModels;

namespace EduMessage.Utils
{
    public class FormattedMessageToUiElementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var message = (FormattedMessage) value;
            return new UIMessageControl(message);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}