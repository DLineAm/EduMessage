using EduMessage.Services;

using Microsoft.Toolkit.Uwp.Notifications;

using MvvmGen.Events;

using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Threading.Tasks;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using EduMessage.ViewModels;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using SignalIRServerTest;

namespace EduMessage
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        public static bool IsAlreadyLaunched;
        public static int OpenedWindows;
        public ObservableCollection<CoreApplicationView> secondaryViews = new();
        private bool _isMainViewClosed;

        [ThreadStatic] public static bool IsDoNotDisturbEnabled;

        private readonly DispatcherTimer _dndTimer;

        //"https://169.254.77.140:5001/"
        //public static string Address = "https://192.168.1.6:5001/";
        //public static string Address = "https://localhost:44347/";

        //public static string Address = "https://169.254.74.100:5001/";
        public static string Address = "https://169.254.77.140:5001/";
        //public static string Address = "https://169.254.74.121:5001/";
        //public static string Address = "https://169.254.74.120:5001/";

        public static IEventAggregator EventAggregator;

        public static ColorManager ColorManager { get; } = new();
        public static Account Account { get; private set; }

        /// <summary>
        /// Инициализирует одноэлементный объект приложения. Это первая выполняемая строка разрабатываемого
        /// кода, поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            _dndTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(1)
            };
            _dndTimer.Tick += delegate { IsDoNotDisturbEnabled = false; _dndTimer.Stop(); };

            UnhandledException += App_UnhandledException;

            CoreApplication.UnhandledErrorDetected += CoreApplication_UnhandledErrorDetected;
        }

        private void CoreApplication_UnhandledErrorDetected(object sender, UnhandledErrorDetectedEventArgs e)
        {
            try
            {
                e.UnhandledError.Propagate();
            }
            catch (Exception exception)
            {
                Trace.WriteLine($"Error: {exception}");
                throw;
            }
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args is ToastNotificationActivatedEventArgs toastEventArgs)
            {
                ToastArguments toastArgs = ToastArguments.Parse(toastEventArgs.Argument);

                if (toastArgs.TryGetValue("action", out var value))
                {
                    if (value.Contains("dnd"))
                    {
                        IsDoNotDisturbEnabled = true;
                        _dndTimer.Start();
                    }
                    if (value.Contains("reply") && toastArgs.TryGetValue("userId", out var recipientIdString))
                    {
                        toastArgs.TryGetValue("conversationId", out var conversationIdString);
                        var recipientId = Convert.ToInt32(recipientIdString);
                        var replyInput = toastEventArgs.UserInput["tbReply"];

                        var message = new Message
                        {
                            IdConversation = string.IsNullOrWhiteSpace(conversationIdString) ? 0 : Convert.ToInt32(conversationIdString),
                            MessageContent = replyInput as string,
                            IdRecipient = recipientId,
                            IdUser = App.Account.GetUser().Id,
                            SendDate = DateTime.Now
                        };

                        var messageAttachments = new MessageAttachment
                        {
                            IdMessageNavigation = message
                        };

                        var list = new List<MessageAttachment> { messageAttachments };

                        var chat = ControlContainer.Get()
                            .Resolve<IChat>();

                        var addResponse = (await (App.Address + "Message/AddMessage")
                                .SendRequestAsync(list, HttpRequestType.Post, App.Account.GetJwt()))
                            .DeserializeJson<int>();

                        if (addResponse != -1)
                        {
                            await chat.SendMessage("SendToUser", addResponse, recipientId);
                        }

                        var aggregator = ControlContainer.Get()
                            .Resolve<IEventAggregator>();
                        await CoreWindow.GetForCurrentThread().Dispatcher
                            .RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                aggregator.Publish(new ReplySentEvent(list, recipientId));

                            });
                    }
                }
            }
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем. Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            OpenedWindows++;
            var container = CreateContainerAndRegisterTypes();

            var featureCollection = container.ResolveConstructor<FeatureCollection>();

            var notificator = container.Resolve<INotificator>("Toast");
            var userBuilder = container.Resolve<IUserBuilder>();

            Account = new Account(userBuilder, notificator);
            EventAggregator = container.Resolve<IEventAggregator>();

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (Window.Current.Content is not Frame rootFrame)
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

            //if (e.PrelaunchActivated == false)
            //{
            if (rootFrame.Content == null)
            {
                // Если стек навигации не восстанавливается для перехода к первой странице,
                // настройка новой страницы путем передачи необходимой информации в качестве параметра
                // навигации
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            else if (IsAlreadyLaunched)
            {
                if (_isMainViewClosed)
                {
                    await secondaryViews[0].Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Window.Current.Activate();
                    });
                    await CreateNewView(e);
                }
                await CreateNewView(e);
                return;
            }
            // Обеспечение активности текущего окна
            Window.Current.Activate();

            var uiSettings = new UISettings();

            uiSettings.ColorValuesChanged += (_, _) =>
            {
                var rgba = uiSettings.GetColorValue(UIColorType.Accent);
                EventAggregator.Publish(new ColorChangedEvent(rgba));
            };

            Window.Current.CoreWindow.Activated += Current_Activated;
        }

        private async Task CreateNewView(LaunchActivatedEventArgs e)
        {
            try
            {
                CoreApplicationView newView = CoreApplication.CreateNewView();
                int newViewId = 0;
                await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    CreateContainerAndRegisterTypes();

                    var frame = new Frame();
                    frame.Loaded += delegate
                    {
                        frame.Navigate(typeof(MainPage));
                    };
                    Window.Current.Content = frame;
                    Window.Current.Activate();


                    secondaryViews.Add(CoreApplication.GetCurrentView());
                    newViewId = ApplicationView.GetForCurrentView().Id;
                });

                int currentViewId = e.CurrentlyShownApplicationViewId;
                bool result = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId, ViewSizePreference.Default, currentViewId, ViewSizePreference.Default);
            }
            catch (Exception ex)
            {

            }
        }

        private ControlContainer CreateContainerAndRegisterTypes()
        {
            var container = ControlContainer.Get();

            container.Register(Component
                .For<IValidator<string>>()
                .ImplementedBy<PasswordValidator>()
                .Named("password"));
            container.Register(Component
                .For<IValidator<string>>()
                .ImplementedBy<LoginValidator>()
                .Named("login"));
            container.Register(Component
                .For<IValidator<string>>()
                .ImplementedBy<PersonNameValidator>()
                .Named("person"));
            container.Register(Component
                .For<IValidator<string>>()
                .ImplementedBy<EmailValidator>()
                .Named("email"));
            container.Register(Component.For<IValidator<string[]>>()
                .ImplementedBy<NullOrWhiteSpaceStringValidator>());

            container.Register(Component.For<IEventAggregator>()
                .ImplementedBy<EventAggregator>()
                .Singleton());

            container.Register(Component.For<IChat>()
                .ImplementedBy<Chat>()
                .Singleton());

            container.Register(Component.For<IUserBuilder>()
                .ImplementedBy<UserBuilder>());

            container.Register(Component.For<INotificator>()
                .ImplementedBy<DialogNotificator>().Named("Dialog"));

            container.Register(Component.For<INotificator>()
                .ImplementedBy<ToastNotificator>().Named("Toast"));

            container.Register(Component.For<IFactory>()
                .ImplementedBy<FeatureFactory>());

            container.Register(Component.For<IMapper>()
                .ImplementedBy<Mapper>());

            container.Register(Component.For<IStorage<MessageAttachment>>()
                .ImplementedBy<MessageStorage>()
                .Singleton());

            var config = new TypeAdapterConfig();

            config.NewConfig<IEnumerable<MessageAttachment>, FormattedMessage>()
                .Map(dest => dest.Message, src => src.FirstOrDefault().IdMessageNavigation)
                .Map(dest => dest.Attachments, src => src.Select(a => a.IdAttachmentNavigation));
            config.NewConfig<IEnumerable<CourseAttachment>, FormattedCourse>()
                .Map(dest => dest.Id, src => src.FirstOrDefault().Id)
                .Map(dest => dest.Attachments, src => src.Select(ca => ca.IdAttachmanentNavigation))
                .Map(dest => dest.Course, src => src.FirstOrDefault().IdCourseNavigation);

            container.RegisterSingleton(config);

            TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = true;
            TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
            TypeAdapterConfig.GlobalSettings.Compile();

            return container;
        }

        private Attachment GetAttachment()
        {
            var random = new Random();
            return new Attachment()
            {
                Title = random.Next(1000, 10000).ToString()
            };
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
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var chat = ControlContainer.Get().Resolve<IChat>();
            await chat.CloseConnection();
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
