
using SignalIRServerTest;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    public class CourseFiles : INotifyPropertyChanged
    {
        private List<EducationFile> _files;
        public Course Course { get; set; }
        public List<EducationFile> Files { get => _files; set { _files = value; OnPropertyChanged();  ChangeFileInfoVisibility(); } }
        private Visibility _filesInfoVisibility;

        private void ChangeFileInfoVisibility()
        {
            FilesInfoVisibility = Files == null
                || Files.Count == 0
                || Files.Count == 1 && Files.First() == null
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public Visibility FilesInfoVisibility { get => _filesInfoVisibility; set { _filesInfoVisibility = value; OnPropertyChanged(); } }
        
        private bool _isAddMode;
        public bool IsAddMode { get => _isAddMode;
            set { _isAddMode = value; OnPropertyChanged(); }
        }

        public IEnumerable<KeyValuePair<EducationFile, int>> TestFiles { get; internal set; }
        public List<int> CoursesId { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string s = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}
