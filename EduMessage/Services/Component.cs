using System;

namespace EduMessage.Services
{
    public class Component
    {
        public Type InterfaceType { get; }
        public Type ImplementationType { get; private set; }
        public string ImplementationName { get; private set; }
        public bool IsSingleton { get; set; }
        public object Object { get; set; }

        private Component(Type interfaceType)
        {
            InterfaceType = interfaceType;
            ImplementationName = interfaceType.FullName;
        }

        public static Component For<TInterface>()
        {
            return new Component(typeof(TInterface));
        }

        public static Component For(Type interfaceType)
        {
            return new Component(interfaceType);
        }

        public Component ImplementedBy<TImplementation>() where TImplementation : class
        {
            ImplementationType = typeof(TImplementation);
            return this;
        }

        public Component ImplementedBy(Type implementationType)
        {
            ImplementationType = implementationType;
            return this;
        }

        public Component Named(string name)
        {
            ImplementationName = name;
            return this;
        }

        public Component Singleton()
        {
            IsSingleton = true;
            return this;
        }
    }
}