using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using EduMessage.Pages;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(INotificator))]
    [Inject(typeof(IEventAggregator))]
    public partial class UserPickViewModel
    {
        [Property] private List<User> _users;
        [Property] private List<School> _schools;
        [Property] [PropertyCallMethod(nameof(SearchSchoolList))] private string _schoolSearchText;
        [Property] private List<Role> _roles;
        [Property] [PropertyCallMethod(nameof(SearchSchools))] private Role _role;
        [Property] [PropertyCallMethod(nameof(SearchSchools))] private string _fullName;
        [Property] private Visibility _noResultsVisualVisibility;

        public async Task Initialize()
        {
            try
            {
                var userResponse = (await (App.Address + "User/All").SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<User>>();

                Users = userResponse;

                UpdateNoResultsVisibility(Users.Count);

                var schoolResponse = (await (App.Address + "Login/Schools").SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<School>>();

                Schools = schoolResponse;

                var roleResponse = (await (App.Address + "Login/Roles").SendRequestAsync<string>(null, HttpRequestType.Get))
                    .DeserializeJson<List<Role>>();

                roleResponse.Insert(0, new Role { Id = -1, Title = "Все роли" });

                Roles = roleResponse;
                Role = roleResponse.First();
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }

        }

        private void UpdateNoResultsVisibility(int usersCount)
        {
            NoResultsVisualVisibility = usersCount == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void SearchSchoolList()
        {
            try
            {
                var response =
                    (await (App.Address + $"Login/Schools.searchText={SchoolSearchText}").SendRequestAsync<string>(null,
                        HttpRequestType.Get)).DeserializeJson<List<School>>();

                Schools = response;
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {

            }
        }

        [Command]
        private async void SearchSchools(object parameter = null)
        {
            if (parameter is not School school)
            {
                school = new School { Id = -1 };
            }

            SchoolSearchText = school.Name;

            try
            {
                var fullName = FullName ?? @"""";
                var userResponse = (await (App.Address + $"User/All.schoolId={school.Id}.roleId={Role.Id}.fullName={fullName}").SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                        .DeserializeJson<List<User>>();

                Users = userResponse;
                UpdateNoResultsVisibility(Users.Count);
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            { }
        }

        [Command]
        private async void StartDialog(object parameter)
        {
            var conversation = new Conversation();
            if (parameter is not User selectedUser) return;

            var conversations = new List<UserConversation>
            {
                new()
                {
                    IdUser = App.Account.GetUser().Id,
                    IdConversationNavigation = conversation,
                    IdUserNavigation = App.Account.GetUser()
                },
                new()
                {
                    IdUser = selectedUser.Id,
                    IdConversationNavigation = conversation,
                    IdUserNavigation = selectedUser
                }
            };

            try
            {
                var foundConversations = (await (App.Address + @$"Message/idUser={selectedUser.Id}&title=""")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<List<UserConversation>>();

                if (foundConversations != null && foundConversations.Count != 0)
                {
                    StartChatNavigation(foundConversations, false);
                    return;
                }

                var response =
                    (await (App.Address + "Message/Add").SendRequestAsync(conversations, HttpRequestType.Post,
                        App.Account.GetJwt(), isLoopHandleIgnore: true))
                    .DeserializeJson<KeyValuePair<int, List<int>>>();
                if (response.Key == -1)
                {
                    return;
                }

                conversation.Id = response.Key;

                conversations.ForEach(c =>
                {
                    c.IdConversation = conversation.Id;
                    c.IdConversationNavigation.Id = conversation.Id;
                });

                for (var i = 0; i < conversations.Count; i++)
                {
                    var newConversationId = response.Value[i];
                    var conversationElement = conversations[i];
                    conversationElement.Id = newConversationId;
                }

                var toRemove = conversations.FirstOrDefault(c => c.IdUser == App.Account.GetUser().Id);

                conversations.Remove(toRemove);

                StartChatNavigation(conversations);
            }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
            catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
            {
                return;
            }
        }

        private void StartChatNavigation(List<UserConversation> conversations,bool notificateNewConversations = true)
        {
            if (notificateNewConversations)
            {
                EventAggregator.Publish(new ConversationGot(conversations));
            }
            new Navigator().Navigate(typeof(ChatPage), conversations, new DrillInNavigationTransitionInfo(), FrameType.MenuFrame);
        }
    }
}