using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

        UIElement Realise(object parameter);
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

        public UIElement Realise(object parameter)
        {
            return null;
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

        public UIElement Realise(object parameter)
        {
            throw new NotImplementedException();
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

        public UIElement Realise(object parameter)
        {
            throw new NotImplementedException();
        }

        public string GetString()
        {
            return FeatureFormatter.GetFormattedString(Prefix);
        }
    }

    public class ItalicTextFeature : IFeature
    {
        public string Prefix => "italic";
        public bool IsDefaultPattern => true;
        public bool ShowInBar => true;
        public Type FeatureType => typeof(TextBlock);
        public IconElement Icon => new SymbolIcon(Symbol.Italic);
        public string Description => "Курсивный шрифт";

        public UIElement Realise(object parameter)
        {
            throw new NotImplementedException();
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