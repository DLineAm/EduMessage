using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EduMessage.Services;
using EduMessage.ViewModels;
using SignalIRServerTest;
using SignalIRServerTest.Models;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private string _userLogin;
        public ChatPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as User;

            if (parameter?.Login == _userLogin)
            {
                return;
            }

            _userLogin = parameter.Login;


            ViewModel = ControlContainer.Get().ResolveConstructor<ChatPageViewModel>();
            var chat = ControlContainer.Get().Resolve<IChat>();
            ViewModel.Initialize(parameter, chat);
            this.DataContext = ViewModel;
        }

        public ChatPageViewModel ViewModel { get; private set; }

        private void ChatView_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var formattedMessage = (FormattedMessage) args.Item;
            var message = formattedMessage.Message;
            args.ItemContainer.HorizontalAlignment = message.IdUser == App.Account.User.Id
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
        }

        private void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (sender is TextBlock {Tag: "Message", DataContext: FormattedMessage formattedMessage} textBlock)
            {
                var message = formattedMessage.Message;
                var messageText = message.MessageContent;
                var regex =
                    "(https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9][a-zA-Z0-9-]+[a-zA-Z0-9]\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|https?:\\/\\/(?:www\\.|(?!www))[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|www\\.[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/])|[a-zA-Z0-9]+\\.[^\\s\\\\]{2,3}([a-zA-Z0-9.\\/&?\\-=]*[a-zA-z0-9\\/]))";
                var links = Regex.Matches(messageText, regex);

                if (links.Count == 0)
                {
                    return;
                }

                var textBlockLength = 0;

                var inlines = textBlock.Inlines;
                inlines.Clear();

                foreach (Match link in links)
                {
                    var difference = link.Index - textBlockLength;
                    var text = messageText.Substring(textBlockLength, difference);
                    inlines.Add(new Run{Text = text});
                    textBlockLength += text.Length;

                    var linkText = messageText.Substring(link.Index, link.Length);
                    var navigateUri = linkText;
                    if (!linkText.Contains("www.") && !linkText.Contains("http://") && !linkText.Contains("https://"))
                    {
                        navigateUri = linkText.Insert(0, "http://");
                    }
                    var hyperlink = new Hyperlink{NavigateUri = new Uri(navigateUri)};
                    hyperlink.Inlines.Add(new Run{Text = linkText});
                    inlines.Add(hyperlink);

                    textBlockLength += linkText.Length;

                    var linkIndex = links.ToList().IndexOf(link);
                    if (linkIndex + 1 == links.Count)
                    {
                        var lastText = messageText.Substring(link.Index + link.Length);
                        inlines.Add(new Run{Text = lastText});
                    }
                }
            }
        }
    }
}
