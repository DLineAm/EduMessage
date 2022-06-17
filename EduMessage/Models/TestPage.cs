using EduMessage.Annotations;

using SignalIRServerTest.Models;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

#nullable disable

namespace WebApplication1
{
    public partial class TestPage : INotifyPropertyChanged
    {
        private TestType _idTestTypeNavigation;
        private int _idTestType;
        private bool _isAddFunctionEnabled;
        private bool _isOpen;
        private string _errorText;
        private TestVariant _textTestVariant;

        public TestPage()
        {
            TestVariants = new ObservableCollection<TestVariant>();
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public int? IdTestFrame { get; set; }

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

        public int IdTestType
        {
            get => _idTestType;
            set
            {
                if (value == _idTestType) return;
                _idTestType = value;
                OnPropertyChanged();

                if (IdTestType == 3 )
                {
                    TextTestVariant ??= new TestVariant {IsCorrect = true, IdTestPage = Id, IdTestPageNavigation = this};
                    
                    IsAddFunctionEnabled = false;
                    return;
                }

                if (IdTestType == 1)
                {
                    var firstVariant = TestVariants.FirstOrDefault(t => t.IsCorrect is true);
                    foreach (var testVariant in TestVariants)
                    {
                        if (firstVariant != null && testVariant.Code == firstVariant.Code)
                        {
                            continue;
                        }
                        testVariant.UpdateIsCorrect(false);
                    }
                }

                IsAddFunctionEnabled = true;
            }
        }

        [JsonIgnore]
        public virtual TestFrame IdTestFrameNavigation { get; set; }

        public virtual TestType IdTestTypeNavigation
        {
            get => _idTestTypeNavigation;
            set
            {
                if (Equals(value, _idTestTypeNavigation)) return;
                _idTestTypeNavigation = value;
                OnPropertyChanged();

                if (_idTestTypeNavigation != null)
                {
                    IdTestType = _idTestTypeNavigation.Id;
                }
            }
        }

        public virtual ObservableCollection<TestVariant> TestVariants { get; set; }


        [JsonIgnore]
        public bool IsAddFunctionEnabled
        {
            get => _isAddFunctionEnabled;
            set
            {
                if (value == _isAddFunctionEnabled) return;
                _isAddFunctionEnabled = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public TestVariant TextTestVariant
        {
            get => _textTestVariant;
            set
            {
                if (Equals(value, _textTestVariant)) return;
                _textTestVariant = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
