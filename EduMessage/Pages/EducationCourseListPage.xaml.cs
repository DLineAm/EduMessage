using EduMessage.ViewModels;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace EduMessage.Pages
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class EducationCourseListPage : Page
    {
        public EducationCourseListPage()
        {
            this.InitializeComponent();
            ViewModel = App.Container.ResolveConstructor<EducationListPageViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var parameter = e.Parameter as Speciality;

            await ViewModel.Initialize(parameter);
            this.DataContext = ViewModel;
        }

        public EducationListPageViewModel ViewModel{ get; }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await CourseAddDialog.ShowAsync();
        }
    }
}
