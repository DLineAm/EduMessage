using System;
using System.Collections.Generic;
using System.Linq;

namespace EduMessage.Services
{
    public class ControlContainer
    {
        [ThreadStatic] private static ControlContainer _instance;

        private ControlContainer() { }

        public static ControlContainer Get() => _instance ??= new ControlContainer();

        private readonly Dictionary<string, Component> _types = new();
        
        private readonly List<object> _singletonComponents = new();

        public void Register(Component component)
        {
            _types[component.ImplementationName] = component;
        }

        public void RegisterSingleton(object singleton)
        {
            var type = singleton.GetType();
            var component = Component.For(type)
                .ImplementedBy(type)
                .Singleton();
            _types[component.ImplementationName] = component;
            _singletonComponents.Add(singleton);
        }

        private object Resolve(Type type)
        {
            var name = type.FullName;
            return Resolve(name);
        }
        private object Resolve(string name)
        {
            var component = _types[name];
            var concreteType = component.ImplementationType;
            object resolvedObject = null;
            if (component.IsSingleton)
            {
                resolvedObject = _singletonComponents.FirstOrDefault(c => c.GetType() == component.ImplementationType);
                if (resolvedObject == null)
                {
                    resolvedObject = InvokeConstructor(concreteType);
                    _singletonComponents.Add(resolvedObject);
                }
            }
            else
            {
                resolvedObject = InvokeConstructor(concreteType);
            }
            return resolvedObject;
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
}