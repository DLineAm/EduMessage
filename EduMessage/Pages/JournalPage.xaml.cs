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

                NoResultsFoundAnimationVisibility = Visibility.Collapsed;

                var taskIds = courseAttachments
                    .Where(c => c.IsTask is true)
                    .Select(c => c.IdCourseNavigation.IdTask)
                    .Distinct();

                var testIds = courseAttachments
                    .Where(c => c.IsTask is false)
                    .OrderBy(t => t.IdCourseNavigation.Position);

                var tasksCount = taskIds.Count();
                var testsCount = testIds.Count();

                TasksCount = tasksCount + 1;

                for (int i = 0; i < TasksCount + 2 + testsCount; i++)
                {
                    JournalGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = GridLength.Auto
                    });
                }

                var usersIds = courseAttachments.Select(c => c.IdUser)
                    .Distinct()
                    .Where(u => u != null);

                var usersCount = usersIds.Count();

                UsersCount = usersCount + 1;

                for (int i = 0; i < UsersCount + 1; i++)
                {
                    JournalGrid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                }

                //SetRowCount(usersCount);
                //SetColumnCount(tasksCount);


                var testIndex = 0;

                for (int i = 0; i < UsersCount; i++)
                {
                    var isVerticalHeader = i == 0;
                    if (i == 0)
                    {
                        testIndex = 0;
                    }
                    var columnOffset = 0;

                    for (int j = 0; j < tasksCount + 2 + testsCount; j++)
                    {
                        var isHorizontalHeader = j == 0;


                        if (isVerticalHeader && isHorizontalHeader)
                        {
                            var header = CreateElement(string.Empty, null);
                            JournalGrid.Children.Add(header);
                            JournalList.Add(header);
                            continue;
                        }

                        var userId = usersIds.ElementAt(isVerticalHeader ? i : i - 1);

                        var text = string.Empty;

                        if (j == tasksCount + 1 + testsCount)
                        {
                            var attachmentsForUser = response.Where(r => r.IdUser == userId && r.Mark != null)
                                .Select(r => r.IdCourse)
                                .Distinct();
                            var marks = new List<byte>();

                            foreach (var item in attachmentsForUser)
                            {
                                var attachment = response.FirstOrDefault(r => r.IdCourse == item && r.IdUser == userId && r.Mark != null);
                                marks.Add((byte)attachment.Mark);
                            }

                            var averageMark = Math.Round(marks
                                .Average(t => t), 1);

                            text = isVerticalHeader ? string.Empty : $"Среднее\r({averageMark})";
                            var averageResultElement = CreateElement(text, null);
                            Grid.SetColumn((FrameworkElement)averageResultElement, j + 2 + testsCount);
                            Grid.SetRow((FrameworkElement)averageResultElement, i);
                            JournalGrid.Children.Add(averageResultElement);
                            JournalList.Add(averageResultElement);
                            continue;
                        }

                        int? taskId = null;
                        CourseAttachment courseAttachment = null;

                        try
                        {
                            taskId = taskIds.ElementAt(isHorizontalHeader ? j : j - 1 );

                            courseAttachment = courseAttachments.FirstOrDefault(c => c.IdUser == userId && c.IdCourseNavigation.IdTask == taskId);

                        }
                        catch (Exception ex)
                        {

                        }

                        CourseAttachment testId = null;

                        if (taskId == null || courseAttachment == null)
                        {
                            try
                            {
                                testId = testIds.ElementAt(testIndex > testIds.Count() ? testIds.Count() - 1: testIndex);
                            }
                            catch (Exception exc)
                            {
                                testId = new CourseAttachment();
                            }
                        }
                        else
                        {
                            testId = testIds.FirstOrDefault(t =>
                                t.IdUser == userId && t.IdCourse == courseAttachment.IdCourse && t.IsTask == false);
                        }

                        if (isVerticalHeader)
                        {
                            if (taskId != null)
                            {
                                var task = courseAttachments.FirstOrDefault(c => c.IdCourseNavigation.IdTask == taskId);

                                text = task?.IdCourseNavigation.IdCourseTaskNavigation.Description;

                                var element = CreateElement(text, isVerticalHeader);
                                Grid.SetColumn((FrameworkElement)element, j + columnOffset);
                                JournalGrid.Children.Add(element);
                                JournalList.Add(element);
                            }

                            if (testId == null)
                            {
                                continue;
                            }

                            text = testId.IdCourseNavigation.Title;

                            var testElement = CreateElement(text + "\r(Тестирование)", isVerticalHeader);
                            Grid.SetColumn((FrameworkElement)testElement, j + columnOffset);
                            JournalGrid.Children.Add(testElement);
                            JournalList.Add(testElement);

                            testIndex++;
                            //columnOffset++;

                            continue;
                        }

                        if (!isVerticalHeader && !isHorizontalHeader)
                        {
                            if (taskId != null)
                            {
                                text = courseAttachment?.Mark?.ToString() ?? "-";
                                var header = CreateElement(text, null);
                                JournalGrid.Children.Add(header);
                                JournalList.Add(header);
                                Grid.SetRow((FrameworkElement)header, i);
                                Grid.SetColumn((FrameworkElement)header, j + columnOffset);
                            }

                            if (testId == null && courseAttachment?.IdCourseNavigation.IdTestFrameNavigation == null)
                            {
                                continue;
                            }

                            columnOffset++;

                            text = testId?.Mark?.ToString() ?? "-";
                            var testHeader = CreateElement(text, null);
                            JournalGrid.Children.Add(testHeader);
                            JournalList.Add(testHeader);
                            Grid.SetRow((FrameworkElement)testHeader, i);
                            Grid.SetColumn((FrameworkElement)testHeader, j + columnOffset);
                            testIndex += testId?.Mark == null ? 0 : 1;
                            continue;
                        }

                        var user = courseAttachments.FirstOrDefault(c => c.IdUser == userId)
                            .IdUserNavigation;

                        text = $"{user.LastName} {user.FirstName} {user.MiddleName}";

                        var horizontalHeader = CreateElement(text, isVerticalHeader);
                        Grid.SetRow((FrameworkElement)horizontalHeader, i);
                        JournalGrid.Children.Add(horizontalHeader);
                        JournalList.Add(horizontalHeader);
                    }
                }
            }
            catch (Exception exception)
            {

            }
        }

        //private void SetRowCount(int rowCount)
        //{
        //    var panel = JournalListView.ItemsPanelRoot as Grid;
        //    panel.RowDefinitions.Clear();
        //    for (int i = 0; i < rowCount; i++)
        //    {
        //        panel.RowDefinitions.Add(new RowDefinition());
        //    }
        //}

        //private void SetColumnCount(int columnCount)
        //{
        //    var panel = JournalListView.ItemsPanelRoot as Grid;

        //    panel.ColumnDefinitions.Clear();
        //    for (int i = 0; i < columnCount; i++)
        //    {
        //        panel.ColumnDefinitions.Add(new ColumnDefinition());
        //    }
        //}

        private UIElement CreateElement(string text, bool? isVerticalHeader)
        {
            var grid = CreateGrid();
            var textBlock = CreateTextBlock(text);
            grid.Children.Add(textBlock);

            if (isVerticalHeader == null)
            {
                return grid;
            }

            var border = CreateBorder((bool)isVerticalHeader);

            grid.Children.Add(border);

            return grid;
        }

        private Border CreateBorder(bool isVertical)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Colors.White),
                Margin = new Thickness(5)
            };

            if (isVertical)
            {
                Grid.SetRow(border, 1);
                border.Height = 2;
                return border;
            }

            Grid.SetColumn(border, 1);
            border.Width = 2;
            return border;
        }

        private Grid CreateGrid()
        {
            return new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition(),
                    new RowDefinition{ Height = GridLength.Auto}
                },
                ColumnDefinitions =
                {
                    new ColumnDefinition(),
                    new ColumnDefinition{Width = GridLength.Auto},
                },
                Height = 70,
                Width = 150
            };
        }

        private TextBlock CreateTextBlock(string text)
        {
            return new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextAlignment = TextAlignment.Center
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ListGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            var grid = sender as Grid;

            grid.ColumnDefinitions.Clear();
            for (int i = 0; i < TasksCount; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            grid.RowDefinitions.Clear();
            for (int i = 0; i < UsersCount; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
            }
        }
    }
}
