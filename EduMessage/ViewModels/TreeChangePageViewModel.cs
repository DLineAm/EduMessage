using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using EduMessage.Models;
using EduMessage.Pages;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class TreeChangePageViewModel : IEventSubscriber<UiElementDropCompletedEvent>
    {
        [Property] private List<SpecialityTree> _specialityTrees;

        public async Task Initialize()
        {
            try
            {
                var specialities = (await (App.Address + "Login/Specialities")
                        .SendRequestAsync<string>(null, HttpRequestType.Get))
                    .DeserializeJson<IEnumerable<Speciality>>()
                    .Select(s => new SpecialityTree
                    {
                        Id = s.Id,
                        Code = s.Code,
                        Title = s.Title
                    });

                var list = new List<SpecialityTree>();

                foreach (var speciality in specialities)
                {
                    var mainCourses = (await (App.Address + $"Education/Courses.IdSpeciality={speciality.Id}")
                            .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                        .DeserializeJson<List<MainCourse>>()
                        .Select(c => new MainCourseTree
                        {
                            Id = c.Id,
                            Title = c.Title,
                            SpecialityId = speciality.Id
                        });
                    foreach (var course in mainCourses)
                    {
                        var response = (await (App.Address +
                                              $"Education/Courses.IdMainCourse={course.Id}&IncludeProperties=false")
                                .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                            .DeserializeJson<List<Course>>();
                        var courses = response.Select(c => new CourseTree
                        {
                            Id = c.Id,
                            Title = c.Title,
                            MainCourseId = course.Id
                        });
                        course.CourseTrees = new ObservableCollection<CourseTree>(courses);
                        speciality.MainCourseTrees.Add(course);
                    }
                    //speciality.MainCourseTrees = mainCourses.ToList();
                    list.Add(speciality);
                }

                SpecialityTrees = list;
                //var courses = (await (App.Address + $"Education/Courses.All")
                //            .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                //        .DeserializeJson<List<Course>>()
                //        .ToList();

                //var specialityTrees = courses.Select(c => new SpecialityTree
                //{
                //    Id = c.IdMainCourseNavigation.IdSpecialityNavigation.Id,
                //    Code = c.IdMainCourseNavigation.IdSpecialityNavigation.Code,
                //    Title = c.IdMainCourseNavigation.IdSpecialityNavigation.Title
                //});
            }
            catch (System.Exception e)
            {

            }
        }

        public void OnEvent(UiElementDropCompletedEvent eventData)
        {
            var courseId = eventData.CourseId;
            var oldMainCourseId = eventData.OldMainCourseId;
            var newMainCourseId = eventData.NewMainCourse;

            CourseTree courseTree = null; 

            foreach (var specialityTree in SpecialityTrees)
            {
                var oldMainCourse = specialityTree.MainCourseTrees.FirstOrDefault(c => c.Id == oldMainCourseId);
                if (oldMainCourse == null)
                {
                    continue;
                }
                courseTree = oldMainCourse.CourseTrees.FirstOrDefault(c => c.Id == courseId);
                oldMainCourse.CourseTrees.Remove(courseTree);

                
            }

            foreach (var specialityTree in SpecialityTrees)
            {
                var newMainCourse = specialityTree.MainCourseTrees.FirstOrDefault(c => c.Id == newMainCourseId);
                if (newMainCourse == null)
                {
                    continue;
                }
                courseTree.MainCourseId = newMainCourseId;
                newMainCourse.CourseTrees.Add(courseTree);
            }
        }

        [Command]
        private async void Apply()
        {
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Сохранение иерархии"));

            try
            {
                var mainCourses = new List<MainCourseTree>();
                foreach (var specialityTree in SpecialityTrees)
                {
                    mainCourses.AddRange(specialityTree.MainCourseTrees);
                }
                var courseIdToMainCourseIdDictionary = new List<KeyValuePair<int, int>>();

                foreach (var mainCourse in mainCourses)
                {
                    var courses = mainCourse.CourseTrees.Select(c => (c.Id, c.MainCourseId));

                    foreach (var course in courses)
                    {
                        courseIdToMainCourseIdDictionary.Add(new KeyValuePair<int, int>(course.Id, course.MainCourseId));
                    }
                }

                var response = (await (App.Address + "Education/Courses.ChangeMainCourseId")
                    .SendRequestAsync(courseIdToMainCourseIdDictionary, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!response)
                {
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.ClosePane, "Не удалось изменить иерархию!"));
                    //EventAggregator.Publish(new CourseAddedOrChangedEvent(false));
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                }

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Иерархия изменена!"));
                //EventAggregator.Publish(new CourseAddedOrChangedEvent(true));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
            catch (System.Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.ClosePane, "Не удалось изменить иерархию!"));
                //EventAggregator.Publish(new CourseAddedOrChangedEvent(false));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }
    }
}

public class SpecialityTree
{
    public int Id { get; set; }
    public string Code { get; set; }
    public string Title { get; set; }
    public ObservableCollection<MainCourseTree> MainCourseTrees { get; set; } = new();
}

public class MainCourseTree
{
    public int Id { get; set; }
    public string Title { get; set; }
    public int SpecialityId { get; set; }
    public ObservableCollection<CourseTree> CourseTrees { get; set; } = new();
}

public class CourseTree
{
    public int Id { get; set; }
    public int MainCourseId { get; set; }
    public string Title { get; set; }
}
