using EduMessage.Annotations;
using EduMessage.Services;

using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace EduMessage.Resources
{
    public sealed partial class LinkUserControl : UserControl, INotifyPropertyChanged
    {
        private string _firstLinkText;
        private string _siteUrl;
        private object _imagePath;
        private string _siteTitle;

        public LinkUserControl( )
        {
            //_firstLinkText = firstLinkText;
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

        public string SiteUrl
        {
            get => _siteUrl;
            set
            {
                _siteUrl = value;
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

        private async Task LoadSiteInfo(string firstLinkText)
        {
            if (firstLinkText == null) return;
            var lastChar = firstLinkText.Last();
            if (lastChar != '/')
            {
                firstLinkText += '/';
            }

            await Task.WhenAll(LoadSiteTitle(firstLinkText), LoadSitePicture(firstLinkText));
        }

        private async Task LoadSiteTitle(string firstLinkText)
        {
            var client = new WebClient();

            try
            {
                string source = await client.DownloadStringTaskAsync(firstLinkText);
                var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                    RegexOptions.IgnoreCase).Groups["Title"].Value;
                TitleBox.Text = title;
                _siteUrl = firstLinkText;
            }
            catch (Exception e)
            {
                return;
            }
        }

        private async Task LoadSitePicture(string firstLinkText)
        {
            const string iconName = "favicon.ico";
            var iconLink = new Uri(firstLinkText + iconName);
            StorageFile icon = await ApplicationData.Current.LocalFolder.CreateFileAsync(iconName, CreationCollisionOption.GenerateUniqueName);
            var downloader = new BackgroundDownloader();
            DownloadOperation operation = await Task.FromResult(downloader.CreateDownload(iconLink, icon));
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            IProgress<DownloadOperation> progress = new Progress<DownloadOperation>(DownloadProgressChanged);
            try
            {
                await operation.StartAsync().AsTask(cancellationToken.Token, progress);
            }
            catch (Exception e)
            {
                await operation.ResultFile.DeleteAsync();
                operation = null;
            }
        }

        private async void DownloadProgressChanged(DownloadOperation obj)
        {
            try
            {
                var file = obj.ResultFile;
                var data = (await FileIO.ReadBufferAsync(file)).ToArray();
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                SiteImage.Source = await data.CreateBitmap(48);
                //LinkUiVisibility = Visibility.Visible;
            }
            catch (Exception e)
            {

            }
        }

        private async void LinkUserControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            await LoadSiteInfo(_firstLinkText);
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
                const string iconName = "/favicon.ico";
                var iconLink = new Uri(_firstLinkText + iconName);
                CancellationTokenSource cancellationToken = new CancellationTokenSource();

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
            catch (Exception ex)
            {

            }

            //var web = new HtmlWeb();
            //var document = web.Load(iconLink);
            //var el = document.DocumentNode.SelectSingleNode("/html/head/link[@rel='icon' and @href]");

            //var resolver = new FaviconResolver();
            //var content =
            //    await resolver.ResolveFaviconAsync(iconLink, FaviconOption.PreferRootFavicon, cancellationToken.Token);
            
            //var image = await StorageFile.GetFileFromApplicationUriAsync(content.ImageUri);
            //using var stream = await image.OpenStreamForReadAsync();
            //using var memoryStream = new MemoryStream();
            //await stream.CopyToAsync(memoryStream);
            //var bytes = memoryStream.ToArray();
            //ImagePath = await bytes.CreateBitmap();
            //StorageFile icon = await ApplicationData.Current.LocalFolder.CreateFileAsync(iconName, CreationCollisionOption.GenerateUniqueName);
            //var downloader = new BackgroundDownloader();
            //DownloadOperation operation = await Task.FromResult(downloader.CreateDownload(iconLink, icon));
            //IProgress<DownloadOperation> progress = new Progress<DownloadOperation>(DownloadProgressChanged);
            //try
            //{
            //    //Debug.WriteLine(operation.Progress.Status);
            //    await operation.StartAsync().AsTask(cancellationToken.Token, progress);
            //    Debug.WriteLine(operation.Progress.Status);
            //}
            //catch (Exception ex)
            //{
            //    await operation.ResultFile.DeleteAsync();
            //    operation = null;
            //}
        }
    }
}
