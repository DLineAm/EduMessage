
using SignalIRServerTest;

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.UI.Xaml;

namespace EduMessage.ViewModels
{
    public class CourseFiles : INotifyPropertyChanged
    {
        private List<EducationFile> _files;
        public Course Course { get; set; }
        public List<EducationFile> Files { get => _files; set { _files = value; OnPropertyChanged(); } }

        public Visibility FilesInfoVisibility => Files == null
        || Files.Count == 0
        || Files.Count == 1 && Files[0] == null
        ? Visibility.Collapsed
        : Visibility.Visible;
        public List<int> CoursesId { get; internal set; }
        public IEnumerable<KeyValuePair<EducationFile, int>> TestFiles { get; internal set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string s = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}
