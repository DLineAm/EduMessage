using Windows.UI.Xaml;
using EduMessage.Services;
using EduMessage.ViewModels;

using Windows.UI.Xaml.Controls;
using MvvmGen.Events;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ThemeConstructorPage : Page, IEventSubscriber<FeatureAdded>
    {
        public ThemeConstructorPage()
        {
            this.InitializeComponent();
            var container = ControlContainer.Get();
            var collection = container.ResolveConstructor<FeatureCollection>();
            var aggregator = container.Resolve<IEventAggregator>();
            var notificator = container.Resolve<INotificator>("Dialog");
            aggregator.RegisterSubscriber(this);
            ViewModel = new ThemeConstructorPageViewModel(notificator,aggregator, collection);
            ViewModel.Initialize();
            this.DataContext = ViewModel;
        }

        public ThemeConstructorPageViewModel ViewModel { get; }
        public void OnEvent(FeatureAdded eventData)
        {
            var position = eventData.Length;
            DescriptionBox.Focus(FocusState.Programmatic);
            DescriptionBox.SelectionStart = position;
        }
    }
}
