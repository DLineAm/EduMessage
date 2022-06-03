using EduMessage.Models;

using SignalIRServerTest.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Course : INotifyPropertyChanged
    {
        private string _title;
        private string _description;
        public int Id { get; set; }
        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public int? IdMainCourse { get; set; }
        public int? IdTeacher { get; set; }
        public int? IdTask { get; set; }

        public virtual CourseTask IdCourseTaskNavigation { get; set; }
        public virtual MainCourse IdMainCourseNavigation { get; set; }
        public virtual User IdTeacherNavigation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string s = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}
