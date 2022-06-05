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
    [Inject(typeof(INotificator))]
    public partial class TreeChangePageViewModel : IEventSubscriber<UiElementDropCompletedEvent>
    {
        [Property] private List<SpecialityTree> _specialityTrees;

        private List<KeyValuePair<int, int>> _specialityCoursePairs = new();

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
            }
            catch (System.Exception e)
            {

            }
        }

        public void OnEvent(UiElementDropCompletedEvent eventData)
        {
            var recordId = eventData.RecordId;
            var oldParentId = eventData.OldParentId;
            var newParentId = eventData.NewParentId;
            var type = eventData.DataType;

            if (type == typeof(MainCourseTree))
            {
                SpecialityTree newSpeciality = null;
                SpecialityTree oldSpeciality = null;
                MainCourseTree mainCourse = null;

                foreach (var specialityTree in SpecialityTrees)
                {
                    if (specialityTree.Id == newParentId)
                    {
                        newSpeciality = specialityTree;
                    }

                    if (specialityTree.Id != oldParentId) continue;

                    oldSpeciality = specialityTree;
                    mainCourse = oldSpeciality.MainCourseTrees.FirstOrDefault(c => c.Id == recordId);
                }

                if (mainCourse == null)
                {
                    return;
                }

                mainCourse.SpecialityId = newSpeciality.Id;
                oldSpeciality.MainCourseTrees.Remove(mainCourse);
                newSpeciality.MainCourseTrees.Add(mainCourse);

                var mainCoursePair = _specialityCoursePairs.FirstOrDefault(p => p.Key == mainCourse.Id);

                if (mainCoursePair.Key != 0)
                {
                    _specialityCoursePairs.Remove(mainCoursePair);
                }

                _specialityCoursePairs.Add(new KeyValuePair<int, int>(mainCourse.Id, newSpeciality.Id));

                return;
            }

            CourseTree courseTree = null;

            foreach (var specialityTree in SpecialityTrees)
            {
                var oldMainCourse = specialityTree.MainCourseTrees.FirstOrDefault(c => c.Id == oldParentId);
                if (oldMainCourse == null)
                {
                    continue;
                }
                courseTree = oldMainCourse.CourseTrees.FirstOrDefault(c => c.Id == recordId);
                oldMainCourse.CourseTrees.Remove(courseTree);


            }

            foreach (var specialityTree in SpecialityTrees)
            {
                var newMainCourse = specialityTree.MainCourseTrees.FirstOrDefault(c => c.Id == newParentId);
                if (newMainCourse == null)
                {
                    continue;
                }
                courseTree.MainCourseId = newParentId;
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

                var mainCourseResponse = (await (App.Address + "Education/Courses.ChangeMainCourseId")
                    .SendRequestAsync(courseIdToMainCourseIdDictionary, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                var specialityResponse = (await (App.Address + "Education/Courses.ChangeSpecialityId")
                        .SendRequestAsync(_specialityCoursePairs, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!mainCourseResponse || !specialityResponse)
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

        [Command]
        private async void DeleteItem(object parameter)
        {
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Удаление записи"));

            var response = false;

            if (parameter is SpecialityTree speciality)
            {
                response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (response)
                {
                    SpecialityTrees.Remove(speciality);
                }
            }
            else if (parameter is MainCourseTree course)
            {
                response = (await (App.Address + $"Education/Courses.IdMainCourse={course.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (response)
                {
                    foreach (var specialityTree in SpecialityTrees)
                    {
                        specialityTree.MainCourseTrees.Remove(course);
                    }
                }
            }

            if (!response)
            {
                Notificator.Notificate("Не удалось удалить запись", "Запись имеет внешние ключи, из-за которых удалить эту запись невозможно");
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                return;
            }



            //EducationFolders.Remove(parameter);

            EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Запись удалена!"));
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
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
