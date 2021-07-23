using System.Collections.Generic;

using UnityEngine;

namespace BareBones.Services.PropertyTable
{
    public class PropertyTable : IPropertyTable
    {
        private Dictionary<int, Dictionary<string, object>> _properties = new Dictionary<int, Dictionary<string, object>>();

        public void Create<T>(int category, string id, T value)
        {
            Debug.Assert(!_properties.ContainsKey(category) || !_properties[category].ContainsKey(id),
                                "PropertyTable.Create duplicate property created.");

            if (!_properties.TryGetValue(category, out Dictionary<string, object> categoryTable))
            {
                categoryTable = new Dictionary<string, object>();
                _properties[category] = categoryTable;
            }

            categoryTable[id] = value;
        }

        public void Update<T>(int category, string id, T value)
        {
            Debug.Assert(_properties.ContainsKey(category) && _properties[category].ContainsKey(id),
                                "PropertyTable.Update property does not exists.");

            _properties[category][id] = value;
        }

        public T Read<T>(int category, string id, T value)
        {
            Debug.Assert(_properties.ContainsKey(category) && _properties[category].ContainsKey(id),
                                "PropertyTable.Read property does not exists.");

            return (T)_properties[category][id];
        }

        public void Delete<T>(int category, string id)
        {
            Debug.Assert(_properties.ContainsKey(category) && _properties[category].ContainsKey(id),
                                "PropertyTable.Delete property does not exists.");

            var categoryProperties = _properties[category];

            categoryProperties.Remove(id);

            if (categoryProperties.Count == 0)
            {
                _properties.Remove(category);
            }
        }

        public Dictionary<string, object> GetProperties(int category)
        {
            Debug.Assert(_properties.ContainsKey(category));

            return _properties[category];
        }

        public void Clear()
        {
            _properties.Clear();
        }

        public void Add(PropertyTable other)
        {
            foreach (var kvp in other._properties)
            {
                if (_properties.ContainsKey(kvp.Key))
                {
                    Merge(kvp.Key, kvp.Value);
                }
                else
                {
                    Add(kvp.Key, kvp.Value);
                }
            }
        }

        public void Add(int category, Dictionary<string, object> properties)
        {
            Debug.Assert(!_properties.ContainsKey(category), "PropertyTable.Add category already exists.");

            _properties[category] = properties;
        }

        public void Merge(int category, Dictionary<string, object> properties)
        {
            Debug.Assert(_properties.ContainsKey(category), "PropertyTable.Merge category does not exists.");

            var existingPropertyTable = _properties[category];

            foreach (var kvp in properties)
            {
                existingPropertyTable[kvp.Key] = kvp.Value;               
            }
        }
    }
}
