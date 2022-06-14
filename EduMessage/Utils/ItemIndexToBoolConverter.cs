using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace EduMessage.Utils
{
    public class ItemIndexToBoolConverter : IValueConverter
    {
        public ListView ListView { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null || parameter is not string ascending)
            {
                return false;
            }

            int index;

            if (value is ListViewItemPresenter presenter)
            {
                var item = VisualTreeHelper.GetParent(presenter) as ListViewItem;
                var listView = ItemsControl.ItemsControlFromItemContainer(item);

                index = listView.IndexFromContainer(item);

                return ascending == "+" ? index + 1 != ListView.Items.Count : index != 0;
            }

            index = ListView.Items.ToList().FindIndex(i => ((dynamic)i).Id == ((dynamic)value).Id);

            return ascending == "+" ? index + 1 != ListView.Items.Count : index != 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}