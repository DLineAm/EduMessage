using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using EduMessage.ViewModels;

namespace EduMessage.Services
{
    public class ItalicTextFeature : IFeature
    {
        public string Prefix => "ital";
        public bool IsDefaultPattern => true;
        public bool ShowInBar => true;
        public Type FeatureType => typeof(TextBlock);
        public IconElement Icon => new SymbolIcon(Symbol.Italic);
        public string Description => "Курсивный шрифт";

        public UIElement Realise(object parameter,ref UIElement lastElement)
        {
            if (parameter is not (string part, FormattedCourse course))
            {
                return null;
            }
            var run = new Run {Text = " " + part, FontStyle = FontStyle.Italic};

            if (lastElement is TextBlock textBlock)
            {
                textBlock.Inlines.Add(run);
                return null;
            }

            textBlock = new TextBlock{TextWrapping = TextWrapping.Wrap};
            textBlock.Inlines.Add(run);
            return textBlock;
        }

        public string GetString()
        {
            return FeatureFormatter.GetFormattedString(Prefix);
        }
    }
}