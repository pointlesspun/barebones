using System;
using System.Collections.Generic;
using System.Reflection;

namespace BareBones.Services.PropertyTable
{
    public static class PolyProps
    {
        private static Assembly[] _assemblies;
        private static Dictionary<string, Type> _typeMapping;


        private static Type Resolve(string name)
        {
            var result = Type.GetType(name);

            if (result != null)
            {
                return result;
            }

            if (_assemblies == null)
            {
                _assemblies = AppDomain.CurrentDomain.GetAssemblies();
                _typeMapping = new Dictionary<string, Type>();

                foreach (var appAssembly in _assemblies)
                {
                    foreach (var type in appAssembly.GetTypes())
                    {
                        _typeMapping[type.Name.ToLower()] = type;
                    }                       
                }
            }

            return _typeMapping[name.ToLower()];
        }

        public static object CreateInstance(string text, IParseFunction function, Type t = null) 
        {
            var parseResult = function.Parse(text);

            if (parseResult.isSuccess)
            {
                var dictionary = parseResult.value as Dictionary<string, object>;

                if (dictionary != null && dictionary.Count == 1)
                {
                    foreach (var kvp in dictionary)
                    {
                        var key = kvp.Key;
                        var properties = kvp.Value as Dictionary<string, object>;

                        if (key != string.Empty && properties != null)
                        {
                            var type = t ?? Resolve(key);

                            if (type != null)
                            {
                                return properties.MapData(Activator.CreateInstance(type));
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static T CreateInstance<T>(string text, IParseFunction function)
        {
            return (T) CreateInstance(text, function, typeof(T));
        }

    }
}
