using EduMessage.Services;
using EduMessage.ViewModels;

using Microsoft.UI.Xaml.Controls;

using MvvmGen.Events;

using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class UserTaskListPage : Page
    {
        private IEventAggregator _aggregator;
        public UserTaskListPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not int courseId)
            {
                return;
            }

            var aggregator = ControlContainer.Get().Resolve<IEventAggregator>();
            _aggregator = aggregator;
            ViewModel = new UserTaskListPageViewModel(aggregator);
            await ViewModel.Initialize(courseId);
            this.DataContext = ViewModel;
        }

        public UserTaskListPageViewModel ViewModel { get; private set; }

        private void NumberBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            var textBox = numberBox.VisualTreeFindName("InputBox") as TextBox;
            textBox.TextChanged +=TextBoxOnTextChanged;
        }

        private void TextBoxOnTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (sender as TextBox).Text;
            var dataContext = (sender as TextBox).DataContext as FormattedCourseTask;
            bool isNumber = text.All(char.IsDigit);
            if (isNumber)
            {
                byte.TryParse(text, out var value);

                dataContext.GeneralMark = value;

                //_aggregator.Publish(new NumberBoxTextChanged(value));
            }
        }
    }

    public record NumberBoxTextChanged(byte Result);
}
