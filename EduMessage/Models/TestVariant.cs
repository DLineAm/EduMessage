using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using EduMessage.Annotations;

#nullable disable

namespace WebApplication1
{
    public partial class TestVariant : INotifyPropertyChanged
    {
        public Guid Code { get; } = Guid.NewGuid();
        private bool? _isCorrect;
        private string _title;
        public int Id { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public bool? IsCorrect
        {
            get => _isCorrect;
            set
            {
                if (value == _isCorrect) return;
                _isCorrect = value;
                OnPropertyChanged();

                if (IdTestPageNavigation is not {IdTestType: 1} testPage) return;

                var variants = testPage.TestVariants.Where(v => v.Code != Code);
                foreach (var testVariant in variants)
                {
                    testVariant.UpdateIsCorrect(false);
                }
            }
        }

        public void UpdateIsCorrect(bool value)
        {
            _isCorrect = value;
            OnPropertyChanged(nameof(IsCorrect));
        }

        public int? IdTestPage { get; set; }

        [JsonIgnore] public int Type { get; set; }

        [JsonIgnore] public virtual TestPage IdTestPageNavigation { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
