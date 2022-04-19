using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
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
        public ChatPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as User;

            ViewModel = ControlContainer.Get().ResolveConstructor<ChatPageViewModel>();
            var chat = ControlContainer.Get().Resolve<IChat>();
            ViewModel.Initialize(parameter, chat);
            this.DataContext = ViewModel;
        }

        public ChatPageViewModel ViewModel { get; private set; }

        private void ChatView_OnContainerContentChanging(ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            var message = (Message) args.Item;
            args.ItemContainer.HorizontalAlignment = message.IdUser == App.Account.User.Id
                ? HorizontalAlignment.Right
                : HorizontalAlignment.Left;
        }
    }
}
