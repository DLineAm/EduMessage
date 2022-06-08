using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class NotApprovedUsersPageViewModel
    {
        [Property] private ObservableCollection<User> _users;
        [Property] private Visibility _noResultsFoundAnimationVisibility;

        public async Task Initialize()
        {
            try
            {
                var response = (await (App.Address + "User/NotApproved")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<IEnumerable<User>>();

                Users = new ObservableCollection<User>(response);
                Users.CollectionChanged += Users_CollectionChanged;
                NoResultsFoundAnimationVisibility = Users.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void Users_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NoResultsFoundAnimationVisibility = Users.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        [Command]
        private async void Approve(object parameter)
        {
            if (parameter is not User user)
            {
                return;
            }

            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Изменение статуса заявки"));

            try
            {
                var response = (await (App.Address + $"User/Id={user.Id}&ApprovedStatus={true}")
                        .SendRequestAsync("", HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!response)
                {
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось изменить статус заявки!"));
                    EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                    return;
                }

                Users.Remove(user);

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Cтатус заявки изменен!"));
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось изменить статус заявки!"));
            }
            finally
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }
    }
}