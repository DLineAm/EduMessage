using SignalIRServerTest.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EduMessage.Annotations;
using Newtonsoft.Json;
using SignalIRServerTest;

#nullable disable

namespace WebApplication1
{
    public partial class TestFrame : INotifyPropertyChanged
    {
        private bool _isOpen;
        private string _errorText;
        private string _inputTest;
        private bool _isChecked;

        public TestFrame()
        {
            Courses = new HashSet<Course>();
            TestPages = new HashSet<TestPage>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime? EndDate { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public string ErrorText
        {
            get => _errorText;
            set
            {
                if (value == _errorText) return;
                _errorText = value;
                OnPropertyChanged();
            }
        }

        [System.Text.Json.Serialization.JsonIgnore]
        [JsonIgnore]
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                if (value == _isOpen) return;
                _isOpen = value;
                OnPropertyChanged();
            }
        }

        public virtual ICollection<Course> Courses { get; set; }
        public virtual ICollection<TestPage> TestPages { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
