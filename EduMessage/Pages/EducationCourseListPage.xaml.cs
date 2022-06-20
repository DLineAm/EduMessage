using EduMessage.Services;
using EduMessage.ViewModels;

using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using EduMessage.Models;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationCourseListPage : Page, IEventSubscriber<DialogStatusChanged>
    {
        private FeatureCollection _featureCollection;
        private bool _isPageLoaded;
        private bool _isContentDialogSuccess;

        public EducationCourseListPage()
        {
            this.InitializeComponent();
            ViewModel = ControlContainer.Get().ResolveConstructor<EducationListPageViewModel>();
            _featureCollection = ControlContainer.Get().ResolveConstructor<FeatureCollection>();

            var eventAggregator = ControlContainer.Get().Resolve<IEventAggregator>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as MainCourse;

            await ViewModel.Initialize(parameter);
            this.DataContext = ViewModel;

            App.EventAggregator.RegisterSubscriber(this);
        }

        public EducationListPageViewModel ViewModel { get; }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _isPageLoaded = true;
        }

        private void FrameworkElement_OnDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            
            if (sender is not StackPanel{DataContext: FormattedCourse{} course} panel || panel.Children.Count != 0)
            {
                return;
            }

            var description = course.Course.Description;

            const string regex = @"\(\!([a-z]{2,4})\[[a-zA-Z0-9._\-а-яА-Я ?]+\]\)";

            var matches = Regex.Matches(description, regex);

            UIElement last = null;

            var baseFeature = _featureCollection.Features.FirstOrDefault(f =>
                f.IsDefaultPattern == false && f.ShowInBar == false && f.FeatureType == typeof(TextBlock));

            if (matches.Count == 0)
            {
                UIElement dummyElement = new Grid();
                var textBlock = baseFeature.Realise((description, course), ref dummyElement);
                panel.Children.Add(textBlock);
                return;
            }

            int lastMatchIndex = 0;

            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var command = match.Value;
                var firstBracketIndex = command.IndexOf('[');
                var lastBracketIndex = command.IndexOf(']');
                var prefix = command.Substring(2, firstBracketIndex - 2);
                var part = command.Substring(firstBracketIndex + 1, (lastBracketIndex) - (firstBracketIndex + 1));

                if (match.Index != 1 && match.Index != 0)
                {
                    var text = description.Substring(lastMatchIndex, match.Index - lastMatchIndex - 1);

                    try
                    {
                        var element = baseFeature.Realise((text, course), ref last);
                        if (element != null)
                        {
                            panel.Children.Add(element);
                            last = element;
                        }
                        else
                        {
                            panel.Children.Remove(last);
                            panel.Children.Add(last);
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }

                lastMatchIndex = match.Index + match.Length;

                var feature = _featureCollection.Features
                    .FirstOrDefault(f => f.Prefix == prefix);

                var elem = feature?.Realise((part, course), ref last);
                if (elem == null)
                {
                    panel.Children.Remove(last);
                    panel.Children.Add(last);

                    if (i == matches.Count - 1)
                    {
                        var matchIndex = match.Index + match.Length;
                        var lastText = description.Substring(matchIndex, description.Length - matchIndex);
                        var lastElement = baseFeature.Realise((lastText, course), ref last);
                        if (lastElement == null)
                        {
                            panel.Children.Remove(last);
                            panel.Children.Add(last);
                            continue;
                        }
                        panel.Children.Add(lastElement);
                        continue;
                    }
                    continue;
                }
                panel.Children.Add(elem);
                last = elem;

                if (i != matches.Count - 1) continue;

                var endElement = GetStringFromBaseFeature(description, lastMatchIndex, baseFeature, description.Length + 1, course);
                if (endElement == null)
                {
                    return;
                }
                panel.Children.Add(endElement);
                last = elem;
            }
        }

        private static UIElement GetStringFromBaseFeature(string description, int lastMatchIndex, IFeature baseFeature, int matchIndex, FormattedCourse formattedCourse)
        {
            var text = description.Substring(lastMatchIndex, matchIndex - lastMatchIndex - 1);

            UIElement dummyElement = new TextBlock();
            return baseFeature.Realise((text, formattedCourse), ref dummyElement);
        }

        private async void MoreButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (App.Account.GetUser().IdRole != 1)
            {
                return;
            }
            _isContentDialogSuccess = false;
            while (!_isContentDialogSuccess)
            {
                await TaskDialog.ShowAsync();

            }
        }

        private async void MoreButton1_OnClick(object sender, RoutedEventArgs e)
        {

                await TaskDialog.ShowAsync();

        }

        public void OnEvent(DialogStatusChanged eventData)
        {
            _isContentDialogSuccess = eventData.IsSuccess;
            if (_isContentDialogSuccess)
            {
                TaskDialog.Hide();
            }
        }

        private void TaskDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _isContentDialogSuccess = true;
            TaskDialog.Hide();
        }
    }
}
