using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;
using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class UserTaskListPageViewModel
    {
        [Property] private List<FormattedCourseTask> _tasks;
        [Property] private CourseTask _courseTask;
        [Property] private Visibility _noResultsFoundAnimationVisibility;

        public async Task Initialize(int courseId)
        {
            Tasks = await GetAllTasks(courseId);

            _courseTask = Tasks.FirstOrDefault()?.Attachments.FirstOrDefault()?.IdCourseNavigation.IdCourseTaskNavigation;
            NoResultsFoundAnimationVisibility = Tasks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private static async Task<List<FormattedCourseTask>> GetAllTasks(int courseId)
        {
            try
            {
                var response = (await (App.Address + $"Education/Courses/Tasks.IdCourse={courseId}")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<IEnumerable<CourseAttachment>>();

                foreach (var courseAttachment in response)
                {
                    if (courseAttachment.IdAttachmanentNavigation == null)
                    {
                        continue;
                    }
                    await courseAttachment.IdAttachmanentNavigation?.SplitAndGetImage(48);
                }

                var groupedTasks = response.GroupBy(t => t.IdUser);

                var tasks = new List<FormattedCourseTask>();

                foreach (var courseAttachment in groupedTasks)
                {
                    var user = response.FirstOrDefault(t => t.IdUser == courseAttachment.Key).IdUserNavigation;
                    var courseAttachments = groupedTasks.Where(t => t.Key == courseAttachment.Key);
                    var pairs = courseAttachments.Select(t =>
                        new FormattedCourseTask(user, t.Select(a => a).ToList()));
                    tasks.AddRange( pairs);
                }

                return tasks;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        [Command]
        private async void OpenFile(object parameter)
        {
            if (parameter is not Attachment attachment)
            {
                return;
            }

            await attachment.OpenFile();
        }

        [Command]
        private async void ApplyComment(object parameter)
        {
            if (parameter is not FormattedCourseTask formattedCourseTask)
            {
                return;
            }

            var idUser = formattedCourseTask.User.Id;
            var comment = formattedCourseTask.Comment;
            var idCourse = formattedCourseTask.Attachments?.FirstOrDefault()?.IdCourse;

            try
            {
                var result = (await (App.Address +
                                     $"Education/Courses/Tasks.IdCourse={idCourse}&IdUser={idUser}")
                        .SendRequestAsync(comment, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!result)
                {
                    return;
                }

                formattedCourseTask.CommentForInterface = formattedCourseTask.Comment;
            }
            catch (Exception e)
            {
                
            }

            
        }

        [Command]
        private async void Apply(object parameter)
        {
            if (parameter is not FormattedCourseTask formattedCourseTask)
            {
                return;
            }

            var idUser = formattedCourseTask.User.Id;
            var mark = formattedCourseTask.GeneralMark;
            var idCourse = formattedCourseTask.Attachments?.FirstOrDefault()?.IdCourse;

            try
            {
                var result = (await (App.Address +
                                     $"Education/Courses/Tasks.IdCourse={idCourse}&IdUser={idUser}&Mark={mark}")
                        .SendRequestAsync(formattedCourseTask.Comment ?? string.Empty, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!result)
                {
                    return;
                }

                formattedCourseTask.GeneralMarkForInterface = formattedCourseTask.GeneralMark;
            }
            catch (Exception e)
            {
                
            }
        }
    }
}