using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest.Models;
using WebApplication1;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class TestConstructorPageViewModel
    {
        [Property] private ObservableCollection<TestPage> _testPages = new();
        [Property] private IEnumerable<TestType> _testTypes;

        [Property] private TestFrame _testFrame;
        [Property] private DateTime _testDate = DateTime.Now.Date;
        [Property] private TimeSpan _testTime = new(DateTime.Now.Add(new TimeSpan(1, 0, 0)).Hour, 0 ,0);
        private bool _isAddMode;


        public async Task Initialize(TestFrame testFrame)
        {
            if (testFrame == null)
            {
                TestFrame = new TestFrame();
                _isAddMode = true;
            }
            else
            {
                TestFrame = testFrame;
                TestPages = new ObservableCollection<TestPage>(TestFrame.TestPages);
                _isAddMode = false;
            }

            try
            {
                var response = (await (App.Address + "Education/Tests/Types")
                        .SendRequestAsync<string>(null, HttpRequestType.Get, App.Account.GetJwt()))
                    .DeserializeJson<IEnumerable<TestType>>();

                TestTypes = response;
            }
            catch (Exception e)
            {
                
            }
        }

        [Command]
        private void AddTestPage()
        {
            TestPages.Add(new TestPage{IdTestType = 1});
        }

        [Command]
        private void AddTestVariant(object parameter)
        {
            if (parameter is not TestPage {IsAddFunctionEnabled: true} testPage)
            {
                return;
            }

            testPage.TestVariants.Add(new TestVariant{Type = testPage.IdTestType, IdTestPageNavigation = testPage});
        }

        [Command]
        private void DeleteTestPage(object parameter)
        {
            if (parameter is not TestPage testPage)
            {
                return;
            }

            TestPages.Remove(testPage);
        }

        [Command]
        private async void Apply()
        {
            foreach (var testPage in TestPages)
            {
                testPage.IsOpen = false;
            }

            var resultTaskDate = _testDate.Add(_testTime);

            TestFrame.EndDate = resultTaskDate;

            TestFrame.IsOpen = false;

            if ( !CheckForWrongTestFrameData() || !CheckForWrongTestPagesData())
            {
                return;
            }

            foreach (var testPage in TestPages)
            {
                if (testPage.IdTestType == 3)
                {
                    testPage.TestVariants = new ObservableCollection<TestVariant>(new[] {testPage.TextTestVariant});
                }
            }

            TestFrame.TestPages = TestPages;

            if (TestFrame.Id == 0)
            {
                EventAggregator.Publish(new TestFrameDataChanged(TestFrame));
                EventAggregator.Publish(new EducationPageBack());
                return;
            }

            EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Visible, "Сохранение тестирования"));

            try
            {
                var response = (await (App.Address + "Education/Tests")
                        .SendRequestAsync(TestFrame, HttpRequestType.Put, App.Account.GetJwt()))
                    .DeserializeJson<bool>();

                if (!response)
                {
                    EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось изменить тестирование!"));
                    return;
                }

                EventAggregator.Publish(new TestFrameDataChanged(TestFrame));
                EventAggregator.Publish(new EducationPageBack());

                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Accept, "Тестирование изменено!"));
            }
            catch (Exception e)
            {
                EventAggregator.Publish(new InAppNotificationShowing(Symbol.Cancel, "Не удалось изменить тестирование!"));

            }
            finally
            {
                EventAggregator.Publish(new LoaderVisibilityChanged(Visibility.Collapsed, string.Empty));
            }
        }

        private bool CheckForWrongTestPagesData()
        {
            var result = true;
            foreach (var testPage in TestPages)
            {
                
                if (string.IsNullOrWhiteSpace(testPage.Title))
                {
                    testPage.ErrorText = "Поле ввода вопроса должно быть заполнено!";
                    testPage.IsOpen = true;
                    result = false;
                    continue;
                }

                if (testPage.IdTestType != 3 && testPage.TestVariants.All(t => t.IsCorrect is false or null))
                {
                    testPage.ErrorText = "Выберите правильный ответ!";
                    testPage.IsOpen = true;
                    result = false;
                    continue;
                }

                if (testPage.IdTestType == 3)
                {
                    if (string.IsNullOrWhiteSpace(testPage.TextTestVariant.Title))
                    {
                        testPage.ErrorText = "Поле ответа должно быть заполнено!";
                        testPage.IsOpen = true;
                        result = false;
                    }

                    continue;
                }

                if (testPage.TestVariants.Count >= 2)
                {
                    foreach (var testVariant in testPage.TestVariants)
                    {
                        if (!string.IsNullOrWhiteSpace(testVariant.Title)) continue;

                        testPage.ErrorText = "Поля ответов должны быть заполнены!";
                        testPage.IsOpen = true;
                        result = false;
                    }

                    continue;
                }

                testPage.ErrorText = "В этом вопросе должно быть минимум 2 варианта ответа";
                testPage.IsOpen = true;
                result = false;
            }

            return result;
        }

        private bool CheckForWrongTestFrameData()
        {
            if (string.IsNullOrWhiteSpace(TestFrame.Title))
            {
                TestFrame.ErrorText = "Поле названия тестирования должно быть заполнено!";
                TestFrame.IsOpen = true;
                return false;   
            }
            if (TestFrame.EndDate == null)
            {
                TestFrame.ErrorText = "Выберите дату и время окончания тестирования!";
                TestFrame.IsOpen = true;
                return false;   
            }
            if (TestFrame.EndDate != null && TestFrame.EndDate <= DateTime.Now)
            {
                TestFrame.ErrorText = "Дата и время окончания тестирования должны быть больше текцщих!";
                TestFrame.IsOpen = true;
                return false;   
            }

            return true;
        }
    }

    public record TestFrameDataChanged(TestFrame TestFrame);
}