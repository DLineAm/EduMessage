using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;
using EduMessage.ViewModels;

namespace EduMessage.Services
{
    public class ImageFeature : IFeature
    {
        public string Prefix => "img";
        public bool IsDefaultPattern => true;
        public bool ShowInBar => true;
        public Type FeatureType => typeof(Image);
        public IconElement Icon => new SymbolIcon(Symbol.Pictures);
        public string Description => "Добавить изображение в текст";

        public UIElement Realise(object parameter, ref UIElement lastElement)
        {
            if (parameter is not (string part, FormattedCourse course))
            {
                return new Image();
            }

            var attachment = course.Attachments.Where(a => a != null)
                .FirstOrDefault(a => a.Title == part);
            if (attachment == null)
            {
                return new Image();
            }
            var imagePath = attachment.ImagePath;
            BitmapImage source = null;
            if (imagePath is BitmapImage bitmap)
            {
                source = bitmap;
            }
            else if (imagePath is string stringPath)
            {
                source = new BitmapImage(new Uri(stringPath));
            }

            var image = new Image
            {
                Source = source,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 700,
                Margin = new Thickness(0,0,0,12)
            };

            if (lastElement is TextBlock textBlock)
            {
                textBlock.Inlines.Add(new LineBreak());
            }

            return image;
        }

        public string GetString()
        {
            return FeatureFormatter.GetFormattedString(Prefix);
        }
    }
}