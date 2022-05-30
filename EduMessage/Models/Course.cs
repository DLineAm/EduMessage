using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EduMessage.Models;

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

        public virtual MainCourse IdMainCourseNavigation { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string s = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(s));
        }
    }
}
