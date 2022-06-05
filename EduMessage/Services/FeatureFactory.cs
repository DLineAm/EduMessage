using System;

namespace EduMessage.Services
{
    public class FeatureFactory : IFactory
    {
        public object Realise(Type elementType)
        {
            return Activator.CreateInstance(elementType);
        }
    }
}