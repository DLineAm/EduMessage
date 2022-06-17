using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EduMessage.Services;
using MvvmGen;
using SignalIRServerTest.Models;
using WebApplication1;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class TestConstructorPageViewModel
    {
        [Property] private ObservableCollection<TestPage> _testPages = new();
        [Property] private IEnumerable<TestType> _testTypes;
        //[Property] private TestType _selectedTestType;


        public async Task Initialize()
        {
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

            if (!CheckForWrongTestData())
            {
                return;
            }
        }

        private bool CheckForWrongTestData()
        {
            var result = true;
            foreach (var testPage in TestPages)
            {
                if (string.IsNullOrWhiteSpace(testPage.Title))
                {
                    testPage.ErrorText = "Поле ввода вопроса должно быть заполнено!";
                    testPage.IsOpen = true;
                    return false;
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
    }
}