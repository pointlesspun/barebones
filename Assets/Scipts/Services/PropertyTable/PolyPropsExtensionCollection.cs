using System;
using System.Collections.Generic;

using BareBones.Common;

namespace BareBones.Services.PropertyTable
{
    public class PolyPropsExtensionCollection : IPolyPropsExtension
    {
        private List<IPolyPropsExtension> _extensions = new List<IPolyPropsExtension>();

        public PolyPropsExtensionCollection Add(params Type[] extensions)
        {
            foreach (var type in extensions)
            {
                _extensions.Add((IPolyPropsExtension) Activator.CreateInstance(type));
            }

            return this;
        }

        public PolyPropsExtensionCollection Add<T>() where T : IPolyPropsExtension
        {
            _extensions.Add(Activator.CreateInstance<T>());
            return this;
        }

        public bool CanParse(string text, int start, PolyPropsConfig config)
        {
            return _extensions.Any(extension => extension.CanParse(text, start, config));
        }

        public (object value, int charactersRead) Parse(string text, int start, PolyPropsConfig config)
        {
            return _extensions.Find(extension => extension.CanParse(text, start, config))
                        .Parse(text, start, config);
        }

        public static PolyPropsConfig CreateConfig(params Type[] extensions) 
            => CreateConfig(new PolyPropsConfig(), extensions);
        

        public static PolyPropsConfig CreateConfig(PolyPropsConfig config, params Type[] extensions)
        {
            var collection = new PolyPropsExtensionCollection().Add(extensions);

            config.CanParse = collection.CanParse;
            config.Parse = collection.Parse;

            return config;
        }
    }
}
