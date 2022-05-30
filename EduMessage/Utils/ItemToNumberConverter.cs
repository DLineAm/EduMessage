using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using EduMessage.ViewModels;

namespace EduMessage.Utils
{
    public class ItemToNumberConverter : IValueConverter
    {
        public ListView ListView { get; set; }
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return 0;
            }

            var index = ListView.Items.ToList().FindIndex(i => ((dynamic)i).Id == ((dynamic)value).Id);

            //var listViewItem = ListView.ContainerFromItem(value) as ListView;

            //if (listViewItem == null)
            //{
            //    return 0;
            //}

            //var index = ListView.IndexFromContainer(listViewItem);
            return index + 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}