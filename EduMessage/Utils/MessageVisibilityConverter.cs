using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using SignalIRServerTest.Models;

namespace EduMessage.Utils
{
    public class MessageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (App.Account.GetUser() == null)
            {
                return Visibility.Collapsed;
            }
            if (value is not int idUser)
            {
                return Visibility.Visible;
            }
            if (parameter is "upper" && idUser != App.Account.GetUser().Id)
            {
                return Visibility.Visible;
            }
            if (parameter is "bottom" && idUser == App.Account.GetUser().Id)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}