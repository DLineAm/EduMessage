using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.UserProfile;
using Windows.UI;
using EduMessage.Services;
using EduMessage.ViewModels;

using SignalIRServerTest.Models;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using EduMessage.Resources;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ChatPage : Page, IEventSubscriber<MessageChanged>
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

        public Visibility FlyoutMenuItemsVisibility { get; set; }

        private string _userLogin;
        private ScrollViewer _chatScrollViewer;

        public ChatPage()
        {
            this.InitializeComponent();

            FilesBorder.Visibility = Visibility.Collapsed;
            RefactorBorder.Visibility = Visibility.Collapsed;

            FilesBorder.RegisterPropertyChangedCallback(TagProperty, FilesBorderTagChanged);
            RefactorBorder.RegisterPropertyChangedCallback(Border.TagProperty, RefactorBorderTagChanged);

            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();

            aggregator.RegisterSubscriber(this);
        }

        private void ChatScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scroll = _chatScrollViewer.VerticalOffset;
            var absoluteHeight = _chatScrollViewer.ExtentHeight;

            Debug.WriteLine("scroll = " + scroll);
        }

        public ScrollViewer GetVisualChild(DependencyObject parent)
        {
            Border border = VisualTreeHelper.GetChild(parent, 0)
                as Border;
            ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0)
                as ScrollViewer;

            return scrollViewer;
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

        public void OnEvent(MessageChanged eventData)
        {
            //var message = eventData.FoundMessage;

            //var messageFromListView = ChatView.Items.ToList().FindIndex(m => ((FormattedMessage)m).Message.Id == message.Message.Id);

            //var listViewItem = ChatView.ContainerFromIndex(messageFromListView) as ListViewItem;

            //var mainGrid = listViewItem.ContentTemplateRoot as Grid;

            //var innerGrid = mainGrid.Children.FirstOrDefault(c => c.GetType() == typeof(Grid)) as Grid;

            //var border = innerGrid.Children.FirstOrDefault() as Border;

            //var messageControl = border.Child as UIMessageControl;

            //messageControl.DataContext = message;
            //messageControl.FormattedMessageContent = message;

            //messageControl.FormatLinks(null);
        }

        private void UIElement_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var grid = sender as Grid;
            var formattedMessage = (FormattedMessage)grid.DataContext;
            var message = formattedMessage.Message;

            FlyoutMenuItemsVisibility =
                message.IdUser == App.Account.GetUser().Id ? Visibility.Visible : Visibility.Collapsed;
            Bindings.Update();
        }

        private void ChatView_OnLoaded(object sender, RoutedEventArgs e)
        {
            _chatScrollViewer = GetVisualChild(ChatView);
            _chatScrollViewer.ViewChanged += ChatScrollViewer_ViewChanged;
        }

        private void ChatView_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var item = ViewModel.Messages.Last().Last();

            var index = ChatView.Items.ToList().FindIndex(i => ((FormattedMessage)i).Message.Id == item.Message.Id);

            var listViewItem = ChatView.ContainerFromIndex(index);
        }
    }
}
