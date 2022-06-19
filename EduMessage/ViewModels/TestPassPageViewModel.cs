using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EduMessage.Services;
using MvvmGen;
using MvvmGen.Events;
using SignalIRServerTest;
using WebApplication1;
using WinRTXamlToolkit.Tools;

namespace EduMessage.ViewModels
{
    [ViewModel]
    [Inject(typeof(IEventAggregator))]
    public partial class TestPassPageViewModel
    {
        [Property] private TestFrame _testFrame;
        [Property] private int _resultPercent;
        [Property] private int _mark;
        public ObservableCollection<KeyValuePair<string, int>> ChartLegendItems { get; } = new();
        private int _courseId;

        public void IniTialize(int userId, Course course)
        {
            TestFrame = course.IdTestFrameNavigation;

            _courseId = course.Id;

            foreach (var testPage in TestFrame.TestPages)
            {
                testPage.TestVariants.ForEach(t => t.IdTestPageNavigation = testPage);
            }
        }

        [Command]
        private async void Apply()
        {
            foreach (var testPage in _testFrame.TestPages)
            {
                testPage.IsOpen = false;
            }

            if (!CheckForWrongTestPagesData())
            {
                return;
            }

            var result = 0d;
            var maxCount = _testFrame.TestPages.Count;

            foreach (var testPage in _testFrame.TestPages)
            {
                if (testPage.IdTestType == 3)
                {
                    result += string.Equals(testPage.FirstTestVariant.InputText, testPage.FirstTestVariant.Title, StringComparison.CurrentCultureIgnoreCase)
                        ? 1
                        : 0;
                    continue;
                }

                var variantsCount = testPage.TestVariants.Count(t => t.IsCorrect is true);
                var successStep = 1 / variantsCount;

                foreach (var testVariant in testPage.TestVariants)
                {
                    result += testVariant.IsCorrect is true && testVariant.IsChecked ? successStep : 0;
                }
            }
            ResultPercent = (int)Math.Round(result * 100 / maxCount);

            if (ResultPercent < 60)
            {
                Mark = 2;
            }
            else if (ResultPercent >= 60 && ResultPercent < 75)
            {
                Mark = 3;
            }
            else if (ResultPercent >= 75 && ResultPercent < 90)
            {
                Mark = 4;
            }
            else
            {
                Mark = 5;
            }

            ChartLegendItems.Add(new KeyValuePair<string, int>("Правильно", _resultPercent));
            ChartLegendItems.Add(new KeyValuePair<string, int>("Неправильно", 100 - _resultPercent));

            try
            {
                var response = (await (App.Address +
                                           $"Education/Tests.IdUser={App.Account.GetUser().Id}&IdCourse={_courseId}&Mark={Mark}")
                            .SendRequestAsync("", HttpRequestType.Post, App.Account.GetJwt()))
                        .DeserializeJson<bool>();
            }
            catch (Exception e)
            {

            }
        }

        [Command]
        private void GoToEducationCourseListPage()
        {
            //EventAggregator.Publish(new EducationPageBack());
            EventAggregator.Publish(new EducationPageBack());
        }

        private bool CheckForWrongTestPagesData()
        {
            var result = true;
            foreach (var testPage in TestFrame.TestPages)
            {

                if (testPage.IdTestType != 3 && testPage.TestVariants.All(t => !t.IsChecked))
                {
                    testPage.ErrorText = "Выберите ответ!";
                    testPage.IsOpen = true;
                    result = false;
                    continue;
                }

                if (testPage.IdTestType == 3 && string.IsNullOrWhiteSpace(testPage.TestVariants.FirstOrDefault().InputText))
                {
                    testPage.ErrorText = "Текстовое поле ответа должно быть заполнено!";
                    testPage.IsOpen = true;
                    result = false;
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
            }

            return result;
        }
    }
}