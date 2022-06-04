using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EduMessage.Annotations;

#nullable disable

namespace SignalIRServerTest
{
    public partial class Speciality : INotifyPropertyChanged
    {
        private string _code;
        private string _title;
        public int Id { get; set; }

        public string Code
        {
            get => _code;
            set
            { 
                _code = value; OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public virtual ICollection<Group> Groups { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
