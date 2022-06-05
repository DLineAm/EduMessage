using EduMessage.Services;

using MvvmGen;
using MvvmGen.Events;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using EduMessage.Models;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    [Inject(typeof(INotificator))]
    public partial class EducationFolderPageViewModel
    {
        [Property] private ObservableCollection<dynamic> _educationFolders;
        [PropertyCallMethod(nameof(Navigate))]        
        [Property] private dynamic _educationFolder;
        [Property] private Visibility _adminInputVisibility = Visibility.Collapsed;
        [Property] private string _code;
        [Property] private string _title;
        [Property] private Visibility _codeInputVisibility;
        [Property] private string _errorText;
        [Property] private bool _infoBarIsOpen;

        private object _itemToChange;
        private Type _folderType;
        private IValidator<string[]> _validator;

        private int _specialityId;

        public async Task Initialize(object item, IValidator<string[]> validator)
        {
            _validator = validator;
            await Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                async () =>
                {
                    var userRoleId = App.Account.GetUser().IdRole;
                    if (userRoleId == 3)
                    {
                        AdminInputVisibility = Visibility.Visible;
                    }
                    try
                    {
                        if (item is not Speciality speciality)
                        {
                            var response = (await (App.Address + "Login/Specialities")
                                    .SendRequestAsync<string>(null, HttpRequestType.Get))
                                .DeserializeJson<List<Speciality>>()
                                .Cast<dynamic>()
                                .ToList();

                            EducationFolders = new ObservableCollection<dynamic>(response);
                            _folderType = typeof(Speciality);
                            return;
                        }

                        _folderType = typeof(MainCourse);
                        _specialityId = speciality.Id;
                        var courses = (await (App.Address + $"Education/Courses.IdSpeciality={speciality.Id}")
                                .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                            .DeserializeJson<List<MainCourse>>()
                            .Cast<dynamic>()
                            .ToList();

                        EducationFolders = new ObservableCollection<dynamic>(courses);
                    }
#pragma warning disable CS0168 // Переменная "e" объявлена, но ни разу не использована.
                    catch (Exception e)
#pragma warning restore CS0168 // Переменная "e" объявлена, но ни разу не использована.
                    {

                
                    }
                });
            
        }

        [Command]
        private async void DeleteItem(object parameter)
        {
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Удаление записи"));

            var response = false;

            if (parameter is Speciality speciality)
            {
                response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt()))
                    .DeserializeJson<bool>();
            }
            else if (parameter is MainCourse course)
            {
                response = (await (App.Address + $"Education/Courses.IdMainCourse={course.Id}")
                        .SendRequestAsync<string>(null, HttpRequestType.Delete, App.Account.GetJwt()))
                    .DeserializeJson<bool>();
            }

            if (!response)
            {
                Notificator.Notificate("Не удалось удалить запись", "Запись имеет внешние ключи, из-за которых удалить эту запись невозможно");
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
                return;
            }

            EducationFolders.Remove(parameter);

            EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Запись удалена!"));
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
        }

        [Command]
        private void SetItemToChange(object item)
        {
            _itemToChange = item;
            if (item is null)
            {
                CodeInputVisibility = _folderType == typeof(Speciality) ? Visibility.Visible : Visibility.Collapsed;
                Title = null;
                Code = null;
                return;
            }
            CodeInputVisibility = item is Speciality ? Visibility.Visible : Visibility.Collapsed;

            Title = ((dynamic) _itemToChange).Title;
            if (item is Speciality speciality)
            {
                Code = speciality.Code;
            }
        }

        [Command]
        private async void Apply()
        {
            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible,
                (_itemToChange == null ? "Добавление " : "Изменение") + " записи"));

            try
            {
                bool isSpeciality = _folderType == typeof(Speciality);
                var strings = new List<string>();
                if (isSpeciality)
                {
                    strings.Add(Code);
                }
                strings.Add(Title);

                var validatorResponse = _validator.Validate(strings.ToArray());

                if (!validatorResponse)
                {
                    InfoBarIsOpen = true;
                    ErrorText = "Все поля должны быть заполнены!";
                    return;
                }

                bool response;

                switch (_itemToChange)
                {
                    case Speciality speciality:
                        speciality.Code = Code;
                        speciality.Title = Title;
                        response = (await (App.Address + "Education/Courses.ChangeSpeciality")
                                .SendRequestAsync(speciality, HttpRequestType.Put, App.Account.GetJwt()))
                            .DeserializeJson<bool>();
                        if (!response)
                        {
                            InfoBarIsOpen = true;
                            ErrorText = "Не удалось изменить специальность";
                            return;
                        }

                        EventAggregator.Publish(new DialogStatusChanged(true));
                        break;
                    case MainCourse mainCourse:
                        mainCourse.Title = Title;
                        response = (await (App.Address + "Education/Courses.ChangeMainCourse")
                                .SendRequestAsync(mainCourse, HttpRequestType.Put, App.Account.GetJwt()))
                            .DeserializeJson<bool>();
                        if (!response)
                        {
                            InfoBarIsOpen = true;
                            ErrorText = "Не удалось изменить дисциплину";
                            return;
                        }
                        EventAggregator.Publish(new DialogStatusChanged(true));

                        break;
                    default:
                        var item = CreateItem(_folderType, Title, Code);
                        int addResponse;
                        if (_folderType == typeof(Speciality))
                        {
                            addResponse = (await (App.Address + "Education/Courses.AddSpeciality")
                                    .SendRequestAsync(item, HttpRequestType.Post, App.Account.GetJwt()))
                                .DeserializeJson<int>();
                        }
                        else
                        {
                            addResponse = (await (App.Address + "Education/Courses.AddMainCourse")
                                    .SendRequestAsync(item, HttpRequestType.Post, App.Account.GetJwt()))
                                .DeserializeJson<int>();
                        }

                        if (addResponse == -1)
                        {
                            InfoBarIsOpen = true;
                            ErrorText = "Не удалось добавить " + (item is Speciality ? "специальность" : "курс");
                            return;
                        }

                        ((dynamic) item).Id = addResponse;
                        EducationFolders.Add(item);
                        EventAggregator.Publish(new DialogStatusChanged(true));

                        break;
                }

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Запись " +
                    (_itemToChange == null ? "добавлена!" : "изменена!")));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось " +
                    (_itemToChange == null ? "добавить " : "изменить ") + "запись!"));
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }
        //DialogStatusChanged(bool IsSuccess);

        private object CreateItem(Type type, string title, string code)
        {
            if (type == typeof(Speciality))
            {
                return new Speciality
                {
                    Code = code,
                    Title = title
                };
            }

            return new MainCourse()
            {
                Title = title,
                IdSpeciality = _specialityId
            };
        }

        [Command]
        private void OpenTreeChangePage()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedEvent(App.Account.GetUser()));
        }

        private void Navigate()
        {
            EventAggregator.Publish(new SelectedSpecialityChangedEvent(EducationFolder));
        }
    }
}
