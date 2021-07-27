﻿using System;
using System.Collections.Generic;

using BareBones.Common;

namespace BareBones.Services.PropertyTable
{
    public class PolyPropsExtensionCollection : IPolyPropsParseFunction
    {
        private List<IPolyPropsParseFunction> _extensions = new List<IPolyPropsParseFunction>();

        public PolyPropsExtensionCollection Add(params Type[] extensions)
        {
            foreach (var type in extensions)
            {
                _extensions.Add((IPolyPropsParseFunction) Activator.CreateInstance(type));
            }

            return this;
        }

        public PolyPropsExtensionCollection Add(params IPolyPropsParseFunction[] extensions)
        {
            _extensions.AddRange(extensions);
            return this;
        }

        public PolyPropsExtensionCollection Add<T>() where T : IPolyPropsParseFunction
        {
            _extensions.Add(Activator.CreateInstance<T>());
            return this;
        }

        public bool CanParse(string text, int start)
        {
            return _extensions.Any(extension => extension.CanParse(text, start));
        }

        public ParseResult Parse(string text, int start)
        {
            return _extensions.Find(extension => extension.CanParse(text, start))
                        .Parse(text, start);
        }

        public static PolyPropsConfig CreateConfig(params Type[] extensions) 
            => CreateConfig(new PolyPropsConfig(), extensions);


        public static PolyPropsConfig CreateConfig(PolyPropsConfig config, params IPolyPropsParseFunction[] extensions)
        {
            config.ParseExtensions = new PolyPropsExtensionCollection().Add(extensions);
            return config;
        }
            
        public static PolyPropsConfig CreateConfig(PolyPropsConfig config, params Type[] extensions)
        {
            config.ParseExtensions = new PolyPropsExtensionCollection().Add(extensions);
            return config;
        }
    }
}
