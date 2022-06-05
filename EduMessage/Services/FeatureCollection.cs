using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
}