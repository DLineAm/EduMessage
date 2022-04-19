using EduMessage.Services;

using Microsoft.AspNetCore.SignalR.Client;

using MvvmGen.Events;

using System;
using System.Diagnostics;
using System.Net.Http;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.Notifications;
using SignalIRServerTest.Models;

namespace EduMessage
{
    public record ColorChangedEvent(Color Color);
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        //"https://169.254.77.140:5001/"
        public static string Address = "https://192.168.1.6:5001/";

        //public static string Address = "https://169.254.77.140:5001/";

        public static IEventAggregator EventAggregator;

        public static ColorManager ColorManager { get; } = new();
        //public static ControlContainer Container { get; } = ControlContainer.Get();
        public static Account Account { get; private set; }

        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода, поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args is ToastNotificationActivatedEventArgs toastEventArgs)
            {
                ToastArguments toastArgs = ToastArguments.Parse(toastEventArgs.Argument);
                if (toastArgs.Contains("dnd"))
                {
                    
                }

                if (toastArgs.TryGetValue("action", out var value))
                {
                    if (value.Contains("reply") && toastArgs.TryGetValue("userId", out var recipientIdString))
                    {
                        var recipientId = Convert.ToInt32(recipientIdString);
                        var replyInput = toastEventArgs.UserInput["tbReply"];
                        var chat = ControlContainer.Get().Resolve<IChat>();
                        chat.SendMessage("SendToUser",recipientId, replyInput);

                        var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
                        aggregator.Publish(new ReplySentEvent(replyInput.ToString(), recipientId));
                    }
                    
                }
            }
        }

        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            base.OnBackgroundActivated(args);
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем. Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var clientHandler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (_, _, _, _) => true };
            var client = new HttpClient(clientHandler);
            var connection = new HubConnectionBuilder()
                .WithUrl(App.Address + "chat")
                .Build();

            connection.On<string>("SendMessage", s => Debug.WriteLine(s));

            try
            {
                await connection.StartAsync();
                await connection.InvokeAsync("SendMessage", "Hello world");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var container = ControlContainer.Get();

            container.Register(Component
                .For<IValidator>()
                .ImplementedBy<PasswordValidator>()
                .Named("password"));
            container.Register(Component
                .For<IValidator>()
                .ImplementedBy<LoginValidator>()
                .Named("login"));
            container.Register(Component
                .For<IValidator>()
                .ImplementedBy<PersonNameValidator>()
                .Named("person"));
            container.Register(Component
                .For<IValidator>()
                .ImplementedBy<EmailValidator>()
                .Named("email"));

            container.Register(Component.For<IEventAggregator>()
                .ImplementedBy<EventAggregator>()
                .Singleton());

            container.Register(Component.For<IUserBuilder>()
                .ImplementedBy<UserBuilder>());

            container.Register(Component.For<INotificator>()
                .ImplementedBy<DialogNotificator>());

            container.Register(Component.For<IChat>()
                .ImplementedBy<Chat>()
                .Singleton());

            Account = container.ResolveConstructor<Account>();
            EventAggregator = container.Resolve<IEventAggregator>();

            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // навигации
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();
            }       

            var uiSettings = new Windows.UI.ViewManagement.UISettings();
            var color = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);

            uiSettings.ColorValuesChanged += (_, _) =>
            {
                var rgba = uiSettings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Accent);
                EventAggregator.Publish(new ColorChangedEvent(rgba));
            };

            Window.Current.CoreWindow.Activated += Current_Activated;
        }

        private void Current_Activated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                System.Diagnostics.Debug.WriteLine("Deactivated " + DateTime.Now);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Activated " + DateTime.Now);
            }
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }

    public record ReplySentEvent(string Message, int recipientId);
}
