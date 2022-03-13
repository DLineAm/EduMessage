using EduMessage.Services;

using MvvmGen;

using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;

namespace EduMessage.ViewModels
{
    [ViewModel]
    public partial class EducationListPageViewModel
    {
        [Property] private ObservableCollection<CourseAttachment> _courses;
        [Property] private ObservableCollection<EducationFile> _files = new ObservableCollection<EducationFile>();
        [Property] private bool _isClearButtonEnabled;
        [Property] private string _courseTitle;
        [Property] private string _courseDescription;
        private Speciality _speciality;

        public async Task Initialize(Speciality speciality)
        {
            App.DropCompleted += App_DropCompleted;
            _speciality = speciality;

            try
            {
                var response = (await (App.Address + $"Education/Courses.SpecialityId={speciality.Id}")
                    .GetStringAsync(App.Account.Jwt))
                    .DeserializeJson<List<CourseAttachment>>();

                Courses = new ObservableCollection<CourseAttachment>( response);
            }
            catch (Exception e)
            {

            }
        }

        [Command]
        private void CleanFilesList()
        {
            Files.Clear();
            UpdateAccessibility();
        }

        [Command]
        private async void Apply()
        {
            if (string.IsNullOrWhiteSpace(CourseTitle)
                || string.IsNullOrWhiteSpace(CourseDescription))
            {
                //await new ContentDialog
                //{
                //    Title = "Ошибка добавления курса",
                //    Content = "Не удалось добавить курс: поле названия и описания должны быть заполнены",
                //    PrimaryButtonText = "Ok"
                //}.ShowAsync();

                return;
            }

            try
            {
                var course = new CourseAttachment
                {
                    IdCourseNavigation = new Course
                    {
                        Title = CourseTitle,
                        Description = CourseDescription,
                        IdSpecialityNavigation = _speciality
                    }

                };
                var response = (await (App.Address + "Education/Courses")
                    .PostBoolAsync(course, App.Account.Jwt))
                    .DeserializeJson<int>();
                if (response != -1)
                {
                    course.IdCourseNavigation.Id = response;
                    Courses.Add(course);
                    Cancel();
                }
            }
            catch (Exception e)
            {

                
            }
        }

        [Command]
        private void Cancel()
        {
            CourseTitle = string.Empty;
            CourseDescription = string.Empty;
            Files.Clear();
        }


        [Command]
        private void DeleteFile(object file)
        {
            var convertedFile = file as EducationFile;
            Files.Remove(convertedFile);
            UpdateAccessibility();
        }

        private void UpdateAccessibility()
        {
            IsClearButtonEnabled = Files.Count > 0;
        }

        private async void App_DropCompleted(IReadOnlyList<Windows.Storage.IStorageItem> items)
        {
            var files = await ReadFiles(items);

            foreach (var file in files)
            {
                Files.Add(file);
            }

            UpdateAccessibility();
        }

        private async Task<List<EducationFile>> ReadFiles(IReadOnlyList<Windows.Storage.IStorageItem> items)
        {
            var result = new List<EducationFile>();
            foreach (var item in items)
            {
                if (item is StorageFolder folder)
                {
                    var files = await ReadFiles(await folder.GetFilesAsync());
                    result.AddRange(files);
                }
                else if (item is StorageFile file)
                {
                    var buffer = await FileIO.ReadBufferAsync(file);
                    var data = buffer.ToArray();

                    result.Add(new EducationFile
                    {
                        Name = file.Name,
                        Type = file.FileType,
                        Data = data,
                        ImagePath = GetImage(file.FileType)
                    }) ;
                }
            }

            return result;
        }

        private string GetImage(string type) => type switch
        {
            ".docx" => "ms-appx:///Assets/word.png",
            ".pdf" => "ms-appx:///Assets/pdf.png",
            _ => "ms-appx:///Assets/file.png"
        };
    }

    public class EducationFile
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ImagePath { get; set; }

        public byte[] Data { get; set; }
    }
}
