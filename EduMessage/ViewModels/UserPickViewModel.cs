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
using SignalIRServerTest;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(INotificator))]
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
                var userResponse = (await (App.Address + "User/All").SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                    .DeserializeJson<List<User>>();

                Users = userResponse;

                UpdateNoResultsVisibility(Users.Count);

                var schoolResponse = (await (App.Address + "Login/Schools").SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                    .DeserializeJson<List<School>>();

                Schools = schoolResponse;

                var roleResponse = (await (App.Address + "Login/Roles").SendRequestAsync("", HttpRequestType.Get))
                    .DeserializeJson<List<Role>>();

                roleResponse.Insert(0, new Role { Id = -1, Title = "Все роли" });

                Roles = roleResponse;
                Role = roleResponse.First();
            }
            catch (Exception e)
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
                    (await (App.Address + $"Login/Schools.searchText={SchoolSearchText}").SendRequestAsync("",
                        HttpRequestType.Get)).DeserializeJson<List<School>>();

                Schools = response;
            }
            catch (Exception e)
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
                //User/All.schoolId={school.Id}.roleId={Role.Id}.fullName={FullName}
                var fullName = FullName ?? @"""";
                var userResponse = (await (App.Address + $"User/All.schoolId={school.Id}.roleId={Role.Id}.fullName={fullName}").SendRequestAsync("", HttpRequestType.Get, App.Account.Jwt))
                        .DeserializeJson<List<User>>();

                Users = userResponse;
                UpdateNoResultsVisibility(Users.Count);
            }
            catch (Exception e)
            {

            }
        }

        [Command]
        private void StartDialog(object parameter)
        {
            if (parameter is User selectedUser)
            {
                new Navigator().Navigate(typeof(ChatPage), selectedUser, new DrillInNavigationTransitionInfo(), FrameType.MenuFrame);

            }
        }
    }
}