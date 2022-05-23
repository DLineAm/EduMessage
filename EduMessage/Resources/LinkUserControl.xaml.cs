using EduMessage.Annotations;
using EduMessage.Services;

using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace EduMessage.Resources
{
    public sealed partial class LinkUserControl : UserControl, INotifyPropertyChanged
    {
        private string _firstLinkText;
        private object _imagePath;
        private string _siteTitle;

        public LinkUserControl( )
        {
            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        public object ImagePath
        {
            get => _imagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged();
            }
        }

        public string FirstLinkText
        {
            get => _firstLinkText;
            set
            {
                _firstLinkText = value;
                OnPropertyChanged();
            }
        }

        public string SiteTitle
        {
            get => _siteTitle;
            set
            {
                _siteTitle = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void SiteImage_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                const string faviconSite = "https://www.google.com/s2/favicons?sz=32&domain_url=";
                const string iconName = "/favicon.ico";
                //var iconLink = new Uri(_firstLinkText + iconName);
                var iconLink = new Uri(faviconSite + _firstLinkText);

                var request = WebRequest.Create(iconLink);
                var response = await request.GetResponseAsync();

                if (response.ContentLength > 0)
                {
                    using var stream = response.GetResponseStream();
                    using var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    var bytes = memoryStream.ToArray();
                    ImagePath = await bytes.CreateBitmap();
                }
            }
            catch (Exception ex){ }
        }

        private async void LinkBorder_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(_firstLinkText));
        }
    }
}
