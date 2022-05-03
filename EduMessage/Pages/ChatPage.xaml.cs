using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using EduMessage.Services;
using EduMessage.ViewModels;

using SignalIRServerTest.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ChatPage : Page
    {
        private SymbolIcon _sendSymbolIcon = new (Symbol.Send)
        {
            Transitions = new TransitionCollection
            {
                new EntranceThemeTransition()
            },
            Foreground = new SolidColorBrush(Colors.White)
        };
        private SymbolIcon _acceptSymbolIcon = new (Symbol.Accept)
        {
            Transitions = new TransitionCollection
            {
                new EntranceThemeTransition()
            },
            Foreground = new SolidColorBrush(Colors.White)
        };

        private string _userLogin;
        public ChatPage()
        {
            this.InitializeComponent();

            FilesBorder.Visibility = Visibility.Collapsed;
            RefactorBorder.Visibility = Visibility.Collapsed;

            FilesBorder.RegisterPropertyChangedCallback(TagProperty, FilesBorderTagChanged);
            RefactorBorder.RegisterPropertyChangedCallback(Border.TagProperty, RefactorBorderTagChanged);
        }

        public static FormattedMessage GetFormattedMessage(object parameter)
        {
            if (parameter is FormattedMessage message)
            {
                return message;
            }

            return new FormattedMessage();
        }

        private async void RefactorBorderTagChanged(DependencyObject sender, DependencyProperty dp)
        {
            var tag = ((Border) sender).Tag;

            if (tag is not bool isFadeAnimation) return;

            if (isFadeAnimation)
            {
                RefactorBorderAppearenceStoryboard.Begin();
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                RefactorBorder.Visibility = Visibility.Collapsed;
                SendButton.Content = _sendSymbolIcon;
                return;
            }

            SendButton.Content = _acceptSymbolIcon;
            RefactorBorder.Visibility = Visibility.Visible;
            DummyRefactorBorderAppearenceStoryboard.Begin();
        }

        private async void FilesBorderTagChanged(DependencyObject sender, DependencyProperty dp)
        {
            var tag = ((Grid) sender).Tag;

            if (tag is not bool isFadeAnimation) return;

            if (isFadeAnimation)
            {
                FilesBorderAppearenceStoryboard.Begin();
                await Task.Delay(TimeSpan.FromMilliseconds(300));
                FilesBorder.Visibility = Visibility.Collapsed;
                AttachmentsExpander.IsExpanded = false;
                return;
            }

            FilesBorder.Visibility = Visibility.Visible;
            DummyFilesBorderAppearenceStoryboard.Begin();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as UserConversation;
            var user = parameter.IdUserNavigation;

            if (user?.Login == _userLogin)
            {
                return;
            }

            _userLogin = user.Login;

            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            var notificator = ControlContainer.Get().Resolve<INotificator>("Dialog");
            ViewModel = new ChatPageViewModel(notificator, aggregator);
            var chat = ControlContainer.Get().Resolve<IChat>();
            await ViewModel.Initialize(parameter, chat);
            this.DataContext = ViewModel;
        }

        public ChatPageViewModel ViewModel { get; private set; }

        private void ChatView_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var formattedMessage = (FormattedMessage) args.Item;
            var message = formattedMessage.Message;
            if(App.Account.GetUser() == null) return;
            args.ItemContainer.HorizontalAlignment = message.IdUser == App.Account.GetUser().Id
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
        }
    }
}
