using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using EduMessage.ViewModels;

namespace EduMessage.Services
{
    public class TitleFeature : IFeature
    {
        public string Prefix => "titl";
        public bool IsDefaultPattern => true;
        public bool ShowInBar => true;
        public Type FeatureType => typeof(TextBlock);
        public IconElement Icon => new SymbolIcon(Symbol.Font);
        public string Description => "Заголовок";

        public UIElement Realise(object parameter,ref UIElement lastElement)
        {
            if (parameter is not (string part, FormattedCourse course))
            {
                return null;
            }

            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap, FontSize = 26, FontWeight = FontWeights.Bold,
                Text = "\n" + part + "\n"
            };
            var border = new Border {Child = textBlock};
            return border;
        }

        public string GetString()
        {
            return FeatureFormatter.GetFormattedString(Prefix);
        }
    }
}