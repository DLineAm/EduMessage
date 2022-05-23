using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.UI.Core;
using EduMessage.Services;
using EduMessage.ViewModels;

using SignalIRServerTest;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Controls;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest.Models;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using NavigationViewDisplayModeChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewDisplayModeChangedEventArgs;
using NavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;
using NavigationViewItemInvokedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewItemInvokedEventArgs;
using NavigationViewSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.NavigationViewSelectionChangedEventArgs;
using PersonPicture = Microsoft.UI.Xaml.Controls.PersonPicture;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public partial class MainMenuPage : Page, IEventSubscriber<ConversationGot>, IEventSubscriber<InAppNotificationShowing>
    {
        public MainMenuPage()
        {
            this.InitializeComponent();


            //NotificationBorder.Visibility = Visibility.Collapsed;

            //AnimatedIcon.SetState(SettingsIcon, "Normal");

            //var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            //aggregator.RegisterSubscriber(this);
        }

 

        public MainMenuViewModel ViewModel { get; private set; }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            //var item = (NavigationViewItem)args.SelectedItem;
        }

        private async void NavigationViewControl_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var invokedItem = args.InvokedItem;
            object item = UnboxItem(invokedItem);

            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    switch (item)
                    {
                        case string itemString
                            when itemString == App.Account.GetUser().FirstName + " " + App.Account.GetUser().LastName:
                            NavFrame.Navigate(typeof(AccountInfoPage));
                            return;
                        case UserConversation conversation:
                            NavFrame.Navigate(typeof(ChatPage), conversation);
                            return;
                        case "Обучение":
                            NavFrame.Navigate(typeof(EducationMainPage));
                            break;
                        case "Параметры":
                            NavFrame.Navigate(typeof(ItemsPickPage));
                            break;
                        default:
                            break;
                    }
                });
        }

        private static object UnboxItem(object invokedItem)
        {
            object item = string.Empty;

            if (invokedItem is StackPanel panel)
            {
                var dataContext = panel.DataContext;

                if (dataContext is MainMenuViewModel menuModel)
                {
                    item = menuModel.AccountName;
                }

                if (dataContext is UserConversation conversation)
                {
                    item = conversation;
                }

                if (dataContext is User)
                {
                    item = dataContext;
                }

                return item;
            }

            item = invokedItem;
            return item;

        }

        private async void NavigationViewControl_DisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args)
        {
            if (App._isAlreadyLaunched)
            {
                return;
            }
            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    const int topIndent = 16;
                    const int expandedIndent = 56;
                    var minimalIndent = 104;

                    if (NavigationViewControl.IsBackButtonVisible.Equals(Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed))
                    {
                        minimalIndent = 58;
                    }

                    var titleBar = MainPage.AppTitleBorder;
                    var currMargin = titleBar.Margin;

                    if (sender.PaneDisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top)
                    {
                        titleBar.Margin = new Thickness(topIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                    }
                    else if (sender.DisplayMode == Microsoft.UI.Xaml.Controls.NavigationViewDisplayMode.Minimal)
                    {
                        titleBar.Margin = new Thickness(minimalIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                    }
                    else
                    {
                        titleBar.Margin = new Thickness(expandedIndent, currMargin.Top, currMargin.Right, currMargin.Bottom);
                    }
                });
            
        }

        private async void MainMenuPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = ControlContainer.Get().ResolveConstructor<MainMenuViewModel>();
            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                NotificationBorder.Visibility = Visibility.Collapsed;

                AnimatedIcon.SetState(SettingsIcon, "Normal");

                var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
                aggregator.RegisterSubscriber(this);

                this.DataContext = ViewModel;
            });
            //this.DataContext = ViewModel;
            await ViewModel.Initialize();

            var response = (await (App.Address + "Message/All")
                    .SendRequestAsync("", HttpRequestType.Get, App.Account.GetJwt()))
                .DeserializeJson<List<UserConversation>>();
            await GenerateNavItems(response);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NavFrame.Navigate(typeof(UserPickPage));
        }

        private async void SettingsNavigationViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AnimatedIcon.SetState(SettingsIcon, "Pressed");
            await Task.Delay(1);
            AnimatedIcon.SetState(SettingsIcon, "Normal");
        }

        public async void OnEvent(ConversationGot eventData)
        {
            var conversations = eventData.Conversations;

            await GenerateNavItems(conversations);
        }

        private async Task GenerateNavItems(List<UserConversation> conversations)
        {

            foreach (var userConversation in conversations)
            {
                var user = userConversation.IdUserNavigation;
                var conversation = userConversation.IdConversationNavigation;
                NavigationViewControl.MenuItems.Add(new NavigationViewItem
                {
                    Content = new StackPanel
                    {
                        DataContext = userConversation,
                        Margin = new Thickness(-35, 0, 0, 0),
                        Orientation = Orientation.Horizontal,
                        Children =
                        {
                            new PersonPicture
                            {
                                Height = 20,
                                ProfilePicture = conversation.Image == null
                                    ? user.Image != null
                                        ? await user.Image.CreateBitmap(36)
                                        : null
                                    : await conversation.Image.CreateBitmap(36)
                            },
                            new TextBlock
                            {
                                Text = conversation.Title ?? user.FirstName + " " + user.LastName,
                                Margin = new Thickness(16, 0, 0, 0)
                            }
                        }
                    }
                });
            }

        }

        public async void OnEvent(InAppNotificationShowing eventData)
        {
            var symbol = eventData.Icon;
            var text = eventData.Text;

            NotificationIcon.Symbol = symbol;
            NotificationTextBlock.Text = text;

            NotificationBorder.Visibility = Visibility.Visible;

            FadeInNotificationStoryboard.Begin();
            await Task.Delay(TimeSpan.FromSeconds(2));
            FadeOutNotificationStoryboard.Begin();
            await Task.Delay(TimeSpan.FromMilliseconds(300));
        }

        private async void MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    var parent = VisualTreeHelper.GetParent(this) as ContentPresenter;
                    (parent.DataContext as MainPageViewModel).SelectedContent = new LoginPage();

                });
        }
    }
}
