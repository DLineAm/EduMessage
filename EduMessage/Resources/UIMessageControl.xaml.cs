using EduMessage.Annotations;
using EduMessage.ViewModels;

using MvvmGen.Commands;

using SignalIRServerTest.Models;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

// Документацию по шаблону элемента "Пользовательский элемент управления" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234236

namespace EduMessage.Resources
{
    public sealed partial class UIMessageControl : UserControl, INotifyPropertyChanged
    {
        private Visibility _attachmentsListVisibility;
        private int _messageId;
        private bool _isMessageFormatted;

        public UIMessageControl(FormattedMessage formattedMessage)
        {
            FormattedMessageContent = formattedMessage;
            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
        }

        public Visibility AttachmentsListVisibility
        {
            get => _attachmentsListVisibility;
            set
            {
                _attachmentsListVisibility = value;
                OnPropertyChanged();
            }
        }

        public FormattedMessage FormattedMessageContent
        {
            get => (FormattedMessage)GetValue(FormattedMessageContentProperty);
            set
            {
                SetValue(FormattedMessageContentProperty, value);

            }
        }

        public async void FormatLinks(object parameter)
        {
            if (_isMessageFormatted)
            {
                return;
            }
            var message = FormattedMessageContent.Message;
                if (message == null)
                {
                    return;
                }

                if (LinkGrid.Children.Any())
                {
                    return;
                }
                

                var messageText = message.MessageContent;
                if (messageText == null)
                {
                    return;
                }

                const string regex =
                    "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/]))";
                var links = Regex.Matches(messageText, regex);

                if (links.Count == 0)
                {
                    return;
                }


                var textBlockLength = 0;

                var inlines = MessageBox.Inlines;
                inlines.Clear();

                string firstLinkText = null;
                foreach (Match link in links)
                {
                    var difference = link.Index - textBlockLength;
                    var text = messageText.Substring(textBlockLength, difference);
                    inlines.Add(new Run { Text = text });
                    textBlockLength += text.Length;

                    var linkText = messageText.Substring(link.Index, link.Length);
                    var navigateUri = linkText;
                    if (!linkText.Contains("www.") && !linkText.Contains("http://") && !linkText.Contains("https://"))
                    {
                        navigateUri = linkText.Insert(0, "http://");
                    }

                    firstLinkText ??= navigateUri;
                    var hyperlink = new Hyperlink { NavigateUri = new Uri(navigateUri) };
                    hyperlink.Inlines.Add(new Run { Text = linkText });
                    inlines.Add(hyperlink);

                    textBlockLength += linkText.Length;

                    var linkIndex = links.ToList().IndexOf(link);

                    if (linkIndex + 1 != links.Count) continue;

                    var lastText = messageText.Substring(link.Index + link.Length);
                    inlines.Add(new Run { Text = lastText });
                }

                LinkUserControl userControl = null;
                try
                {
                    using var client = new WebClient();

                    string source = await client.DownloadStringTaskAsync(firstLinkText);

                    var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                        RegexOptions.IgnoreCase).Groups["Title"].Value;

                    Debug.WriteLine(title);

                    userControl = new LinkUserControl
                    {
                        FirstLinkText = firstLinkText,
                        SiteTitle = title
                    };

                    MessageId = message.Id;
                }
                catch (Exception e)
                {

                }

                _isMessageFormatted = true;
                LinkGrid.Children.Add(userControl);
        }

        public static readonly DependencyProperty FormattedMessageContentProperty =
            DependencyProperty.Register(nameof(FormattedMessageContent),
                typeof(FormattedMessage),
                typeof(UIMessageControl),
                new PropertyMetadata(new FormattedMessage(), PropertyChangedCallback));

        private ICommand _openFileCommand;

        public ICommand OpenFileCommand => _openFileCommand ??= new DelegateCommand(OpenFile);

        public int MessageId
        {
            get => _messageId;
            set
            {
                _messageId = value;
                OnPropertyChanged();
            }
        }

        private ICommand _formatTextCommand;

        public ICommand FormatTextCommand => _formatTextCommand ??= new DelegateCommand(FormatLinks);

        private async void OpenFile(object obj)
        {
            if (obj is not Attachment attachment) return;

            await attachment.OpenFile();
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not FormattedMessage formattedMessage) return;

            var control = d as UIMessageControl;

            if (formattedMessage.Attachments == null)
            {
                control.AttachmentsListVisibility = Visibility.Collapsed;
                return;
            }

            var attachment = formattedMessage.Attachments.FirstOrDefault();

            control.AttachmentsListVisibility = formattedMessage.Attachments == null || formattedMessage.Attachments.Count == 0 ||
                                                attachment == null
                ? Visibility.Collapsed
                : Visibility.Visible;



        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void MessageBox_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            try
            {
                if (sender is not TextBlock { DataContext: FormattedMessage formattedMessage } textBlock)
                {
                    return;
                }
                

                var message = formattedMessage.Message;

                var messageText = message?.MessageContent;
                if (messageText == null)
                {
                    return;
                }

                const string regex =
                    "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/]))";
                var links = Regex.Matches(messageText, regex);

                if (links.Count == 0)
                {
                    return;
                }


                var textBlockLength = 0;

                var inlines = textBlock.Inlines;
                inlines.Clear();

                string firstLinkText = null;
                foreach (Match link in links)
                {
                    var difference = link.Index - textBlockLength;
                    var text = messageText.Substring(textBlockLength, difference);
                    inlines.Add(new Run { Text = text });
                    textBlockLength += text.Length;

                    var linkText = messageText.Substring(link.Index, link.Length);
                    var navigateUri = linkText;
                    if (!linkText.Contains("www.") && !linkText.Contains("http://") && !linkText.Contains("https://"))
                    {
                        navigateUri = linkText.Insert(0, "http://");
                    }

                    firstLinkText ??= navigateUri;
                    var hyperlink = new Hyperlink { NavigateUri = new Uri(navigateUri) };
                    hyperlink.Inlines.Add(new Run { Text = linkText });
                    inlines.Add(hyperlink);

                    textBlockLength += linkText.Length;

                    var linkIndex = links.ToList().IndexOf(link);

                    if (linkIndex + 1 != links.Count) continue;

                    var lastText = messageText.Substring(link.Index + link.Length);
                    inlines.Add(new Run { Text = lastText });
                }

                if (LinkGrid.Children.Count != 0)
                {
                    return;
                }

                LinkUserControl userControl = null;
                try
                {
                    using var client = new WebClient();

                    string source = await client.DownloadStringTaskAsync(firstLinkText);

                    var title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>",
                        RegexOptions.IgnoreCase).Groups["Title"].Value;

                    Debug.WriteLine(title);

                    userControl = new LinkUserControl
                    {
                        FirstLinkText = firstLinkText,
                        SiteTitle = title
                    };

                    MessageId = message.Id;
                }
                catch (Exception e)
                {

                }

                LinkGrid.Children.Add(userControl);
            }
            catch (Exception e)
            {

            }
        }
    }
}
