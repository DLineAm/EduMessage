using EduMessage.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media.Imaging;

namespace EduMessage.Services
{
    public class FeatureCollection
    {
        public List<IFeature> Features { get; } = new();
        private IFactory _featureFactory;

        public FeatureCollection(IFactory featureFactory)
        {
            _featureFactory = featureFactory;
            Features = GetFeatures();
        }

        private List<IFeature> GetFeatures()
        {
            var result = new List<IFeature>();
            var implementations = GetAssignedTypes(typeof(IFeature));

            foreach (var implementation in implementations)
            {
                if (_featureFactory.Realise(implementation) is not IFeature feature)
                {
                    continue;
                }
                result.Add(feature);
            }

            return result;
        }

        private TypeInfo[] GetAssignedTypes(Type interfaceType)
        {
            var assembly = Assembly.GetEntryAssembly();

            var types = (from typeInfo in assembly.DefinedTypes
                         where typeInfo.ImplementedInterfaces.Contains(interfaceType)
                         select typeInfo).ToList();

            return types.ToArray();
        }
    }


    public interface IFactory
    {
        object Realise(Type elementType);
    }

    public class FeatureFactory : IFactory
    {
        public object Realise(Type elementType)
        {
            return Activator.CreateInstance(elementType);
        }
    }

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

            var attachment = course.Attachments.FirstOrDefault(a => a.Title == part);
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

    public static class FeatureFormatter
    {
        public static string GetFormattedString(string prefix)
        {
            return "(!" + prefix + "[])";
        }
    }
}