using System;
using System.Collections.Generic;
using System.Linq;

namespace EduMessage.Services
{
    public class ControlContainer
    {
        private static ControlContainer _instance;

        private ControlContainer()
        {
            
        }

        public static ControlContainer Get()
        {
            return _instance ??= new ControlContainer();
        }

        private readonly Dictionary<string, (Type, Type)> _types = new();

        public void Register(Component component)
        {
            _types[component.ImplementationName] = (component.InterfaceType, component.ImplementationType);
        }

        private object Resolve(Type type)
        {
            var name = type.FullName;
            return Resolve(name);
        }
        private object Resolve(string name)
        {
            var pair = _types[name];
            var concreteType = pair.Item2;
            return InvokeConstructor(concreteType);
        }

        public TInterface Resolve<TInterface>()
        {
            return (TInterface)Resolve(typeof(TInterface));
        }

        public TInterface Resolve<TInterface>(string name)
        {
            return (TInterface)Resolve(name);
        }

        public TImplement ResolveConstructor<TImplement>() where TImplement : class
        {
            var type = typeof(TImplement);
            return (TImplement)InvokeConstructor(type);
        }

        private object InvokeConstructor(Type type)
        {
            var constructor = type.GetConstructors()[0];
            var defaultParameters = constructor.GetParameters();
            var parameters = defaultParameters.Select(p => Resolve(p.ParameterType)).ToArray();
            return constructor.Invoke(parameters);
        }

        public void InvokeMethod(object classObject, string methodName)
        {
            var type = classObject.GetType();
            var method = type.GetMethod(methodName);
            var defaultParameters = method.GetParameters();
            var parameters = defaultParameters.Select(p => Resolve(p.ParameterType)).ToArray();
            method.Invoke(classObject, parameters);
        }

    }

    public class Component
    {
        public Type InterfaceType { get; private set; }
        public Type ImplementationType { get; private set; }
        public string ImplementationName { get; private set; }

        private Component(Type interfaceType)
        {
            InterfaceType = interfaceType;
            ImplementationName = interfaceType.FullName;
        }

        public static Component For<TInterface>()
        {
            return new Component(typeof(TInterface));
        }

        public Component ImplementedBy<TImplementation>() where TImplementation : class
        {
            ImplementationType = typeof(TImplementation);
            
            return this;
        }

        public Component Named(string name)
        {
            ImplementationName = name;
            return this;
        }
    }
}