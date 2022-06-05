using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using EduMessage.ViewModels;

namespace EduMessage.Services
{
    public class BoldTextBlockFeature : IFeature
    {
        public string Prefix => "bold";
        public bool IsDefaultPattern => true;
        public bool ShowInBar => true;
        public Type FeatureType => typeof(TextBlock);
        public IconElement Icon => new SymbolIcon(Symbol.Bold);
        public string Description => "Жирный шрифт";

        public UIElement Realise(object parameter, ref UIElement lastElement)
        {
            if (parameter is not (string part, FormattedCourse course))
            {
                return new Image();
            }
            var run = new Run {Text = " " +  part, FontWeight = FontWeights.Bold};

            if (lastElement is TextBlock textBlock)
            {
                textBlock.Inlines.Add(run);
                return null;
            }

            textBlock = new TextBlock{TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Bold};
            textBlock.Inlines.Add(run);
            return textBlock;
        }

        public string GetString()
        {
            return FeatureFormatter.GetFormattedString(Prefix);
        }
    }
}