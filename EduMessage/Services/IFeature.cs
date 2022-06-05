using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace EduMessage.Services
{
    public interface IFeature
    {
        string Prefix { get; }

        bool IsDefaultPattern { get; }

        bool ShowInBar { get; }

        Type FeatureType { get; }

        IconElement Icon { get; }

        string Description { get; }

        UIElement Realise(object parameter, ref UIElement lastElement);
        string GetString();
    }
}