using EduMessage.Annotations;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace EduMessage.Utils
{
    public class SystemAccentColorSetting : INotifyPropertyChanged
    {
        private readonly UISettings _uiSettings;
        public SystemAccentColorSetting()
        {
            _uiSettings = new UISettings();
            _uiSettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
        }

        private async void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            Color accentColor = sender.GetColorValue(UIColorType.AccentLight1);


            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () => { SystemAccentColor = new SolidColorBrush(accentColor); });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public SolidColorBrush SystemAccentColor
        {
            get => _systemAccentColor;
            set { _systemAccentColor = value; OnPropertyChanged(); }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SolidColorBrush _systemAccentColor = new((Color)Application.Current.Resources["SystemAccentColorLight1"]);

        //private SolidColorBrush _systemAccentColor = new((Color)Application.Current.Resources["AccentFillColorDefaultBrush"]);
    }
}