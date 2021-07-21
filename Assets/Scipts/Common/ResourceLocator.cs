using System;
using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Common
{
    [Serializable]
    public class ResourceLocator
    {
        public static readonly ResourceLocator _instance = new ResourceLocator();

        private Dictionary<string, System.Object> _namedResources = new Dictionary<string, System.Object>();
        private Dictionary<Type, System.Object> _typedResources = new Dictionary<Type, System.Object>();

        private ResourceLocator()
        {
        }

        public T Register<T>()
        {
            return Register<T, T>();
        }

        public void Clear()
        {
            _namedResources.Clear();
            _typedResources.Clear();
        }

        public TClass Register<TInterface, TClass>() where TClass : TInterface
        {
            Debug.Assert(!_typedResources.ContainsKey(typeof(TInterface)),
                "Instance of type " + typeof(TInterface) + " was already registered in the resource locator, the new version will be ignored.");

            if (!_typedResources.ContainsKey(typeof(TInterface)))
            {
                var result = Activator.CreateInstance<TClass>();

                _typedResources[typeof(TInterface)] = result;

                return result;
            }

            return default;
        }
        public T Register<T>(params System.Object[] constructorArgs)
        {
            return Register<T, T>(constructorArgs);
        }

        public TClass Register<TInterface, TClass>(params System.Object[] constructorArgs) where TClass : TInterface
        {
            Debug.Assert(!_typedResources.ContainsKey(typeof(TInterface)),
                "Instance of type " + typeof(TInterface) + " was already registered in the resource locator, the new version will be ignored.");

            if (!_typedResources.ContainsKey(typeof(TInterface)))
            {
                var result = (TClass)Activator.CreateInstance(typeof(TClass), constructorArgs);

                _typedResources[typeof(TInterface)] = result;

                return result;
            }

            return default;
        }

        public T Register<T>(T resource)
        {
            Debug.Assert(!_typedResources.ContainsKey(resource.GetType()),
                "Instance of type " + typeof(T) + " was already registered in the resource locator, the new version will be ignored.");

            if (!_typedResources.ContainsKey(resource.GetType()))
            {
                _typedResources[typeof(T)] = resource;
                return resource;
            }

            return default;
        }

        public void Deregister<T>()
        {
            _typedResources.Remove(typeof(T));
        }

        public void Deregister<T>(T obj) where T : class
        {
            Debug.Assert(
                _typedResources.ContainsKey(typeof(T)) && Resolve<T>() == obj,
                "trying to deregister an object which was not registered with the ResourceLocator."
            );

            if (_typedResources.ContainsKey(typeof(T)) && Resolve<T>() == obj)
            {
                Deregister<T>();
            }
        }

        public T Resolve<T>()
        {
            return (T)_typedResources[typeof(T)];
        }

        public bool Contains<T>()
        {
            return _typedResources.ContainsKey(typeof(T));
        }

        public bool Contains(string name)
        {
            return _namedResources.ContainsKey(name);
        }
    }
}