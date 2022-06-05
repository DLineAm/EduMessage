using System;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using EduMessage.ViewModels;

namespace EduMessage.Services
{
    public class BaseTextBlockFeature : IFeature
    {
        public string Prefix { get; set; }
        public bool IsDefaultPattern { get; set; }
        public bool ShowInBar { get; set; }
        public Type FeatureType => typeof(TextBlock);
        public IconElement Icon { get; set; }
        public string Description { get; }

        public UIElement Realise(object parameter, ref UIElement lastElement)
        {
            if (parameter is not (string part, FormattedCourse course))
            {
                return null;
            }

            if (lastElement is TextBlock textBlock)
            {
                var run = new Run {Text = part, FontWeight = FontWeights.Normal};
                textBlock.Inlines.Add(run);
                return null;
            }

            var element = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Text = part
            };

            return element;
        }

        public string GetString()
        {
            throw new NotImplementedException();
        }

    }
}