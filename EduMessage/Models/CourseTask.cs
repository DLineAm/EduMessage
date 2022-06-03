using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EduMessage.Annotations;

namespace SignalIRServerTest.Models
{
    public class CourseTask : INotifyPropertyChanged
    {
        private string _description;
        private DateTime? _endTime;

        public CourseTask()
        {
            this.Course = new HashSet<Course>();
        }

        public int Id { get; set; }

        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public System.DateTime? EndTime
        {
            get => _endTime;
            set { _endTime = value; OnPropertyChanged(); }
        }

        public virtual ICollection<Course> Course { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}