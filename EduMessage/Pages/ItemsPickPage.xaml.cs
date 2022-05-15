using EduMessage.Resources;

using MvvmGen.Events;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ItemsPickPage : Page
    {
        private IEventAggregator _eventAggregator;
        public SolidColorBrush RectColor { get; private set; }
        public ItemsPickPage()
        {
            this.InitializeComponent();

            ChangeRectColor(false);
        }

        public void Initialize(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private void Rectangle_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;

            ChangeRectColor(true);
        }

        private async void Rectangle_Drop(object sender, DragEventArgs e)
        {

            IReadOnlyList<IStorageItem> items = await e.DataView.GetStorageItemsAsync();

            _eventAggregator.Publish(new DropCompletedEvent(items));

            ChangeRectColor(false);
        }

        private void Rectangle_DragLeave(object sender, DragEventArgs e)
        {
            ChangeRectColor(false);
        }

        private void ChangeRectColor(bool _isEnabledMode)
        {
            if (_isEnabledMode)
            {
                RectColor = new SolidColorBrush((Color) Application.Current.Resources["SystemAccentColor"]);
                Bindings.Update();
                return;
            }
            RectColor = Application.Current.Resources["AccentTextFillColorDisabledBrush"] as SolidColorBrush;
            Bindings.Update();
        }


    }
}
