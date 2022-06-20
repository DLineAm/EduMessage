using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EduMessage.Annotations;
using EduMessage.Services;
using Microsoft.Toolkit.Uwp.UI;
using SignalIRServerTest;
using Grid = Windows.UI.Xaml.Controls.Grid;
using EduMessage.Models;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class JournalPage : Page, INotifyPropertyChanged
    {
        private int _usersCount;
        private int _tasksCount;
        private string _mainCourseTitle;
        private Visibility _noResultsFoundAnimationVisibility;

        public JournalPage()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<UIElement> JournalList { get; set; } = new();

        public int TasksCount
        {
            get => _tasksCount;
            set
            {
                _tasksCount = value;
                OnPropertyChanged();
            }
        }

        public int UsersCount
        {
            get => _usersCount;
            set
            {
                _usersCount = value;
                OnPropertyChanged();
            }
        }

        public Visibility NoResultsFoundAnimationVisibility
        {
            get => _noResultsFoundAnimationVisibility;
            set
            {
                _noResultsFoundAnimationVisibility = value;
                OnPropertyChanged();
            }
        }


        public string MainCourseTitle
        {
            get { return _mainCourseTitle; }
            set { _mainCourseTitle = value; OnPropertyChanged(); }
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not (int idMainCourse, MainCourse mainCourse))
            {
                return;
            }

            MainCourseTitle = mainCourse?.Title;

            try
            {
                var response = (await (App.Address + $"Education/Courses.IdMainCourse={idMainCourse}")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<CourseAttachment>>();
                var courseAttachments = response.Where(c => c.IdUser != null);

                if (!courseAttachments.Any())
                {
                    NoResultsFoundAnimationVisibility = Visibility.Visible;
                    return;
                }

                var coursesIds = courseAttachments
                    .Where(c => c.IdCourse != null && c.IsTask != null)
                    .OrderBy(c => c.IdCourseNavigation.Position)
                    .Select(c => c.IdCourse)
                    .Distinct();

                var courses = coursesIds.Select(coursesId => courseAttachments.FirstOrDefault(c => c.IdCourse == coursesId))
                    .Where(course => course != null).ToList();

                ThemeListView.ItemsSource = courses;

                var usersIds = courseAttachments
                    .Where(c => c.IdCourse != null && c.IdUser != null && c.IsTask != null)
                    .Select(c => c.IdUser)
                    .Distinct();

                var users = usersIds.Select(userId => courseAttachments.FirstOrDefault(c => c.IdUser == userId))
                    .Where(c => c != null)
                    .ToList();

                UsersListView.ItemsSource = users;

                var usersMarks = new List<List<KeyValuePair<int?, int?>>>();

                var averageMarks = new List<double>();

                foreach (var userAttachment in users)
                {
                    var user = userAttachment.IdUserNavigation;

                    var marksCount = 0;

                    var averageMark = 0d;

                    var userMarks = new List<KeyValuePair<int?, int?>>();
                    foreach (var courseAttachment in courses)
                    {
                        var course = courseAttachment.IdCourseNavigation;

                        var testsTasks = courseAttachments.Where(c =>
                            c.IdCourse == course.Id && c.IdUser == user.Id && c.IsTask != null);

                        var test = testsTasks.FirstOrDefault(t => t.IsTask is false);

                        if (test is {Mark: { } taskMark})
                        {
                            averageMark += taskMark;
                            marksCount++;
                        }

                        var task = testsTasks.FirstOrDefault(t => t.IsTask is true);

                        if (task is {Mark: { } testMark})
                        {
                            averageMark += testMark;
                            marksCount++;
                        }

                        userMarks.Add(new KeyValuePair<int?, int?>(task?.Mark, test?.Mark));
                    }

                    averageMark /= marksCount;

                    averageMarks.Add(Math.Round(averageMark,2));

                    usersMarks.Add(userMarks);
                }

                UserMarkListView.ItemsSource = usersMarks;

                AverageMarkListView.ItemsSource = averageMarks;
            }
            catch (Exception exception)
            {

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void TipButton_OnClick(object sender, RoutedEventArgs e)
        {
            MarkTeachingTip.IsOpen = true;
        }
    }
}
