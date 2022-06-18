using EduMessage.Models;

using SignalIRServerTest.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WebApplication1;

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
        public int? Position { get; set; }
        public int? IdTestFrame { get; set; }

        [JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public bool IsExpanded { get; set; }

        public virtual CourseTask IdCourseTaskNavigation { get; set; }
        public virtual MainCourse IdMainCourseNavigation { get; set; }
        public virtual User IdTeacherNavigation { get; set; }
        public virtual TestFrame IdTestFrameNavigation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string s = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}
