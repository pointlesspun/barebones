
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BareBones.Services.PropertyTable
{
    public static class PolyPropsAutoMapper 
    {
        public static T CreateInstance<T>(this Dictionary<string, object> data)
        {
            return MapData<T>(data, Activator.CreateInstance<T>());
        }

        public static T MapData<T>(this Dictionary<string, object> data, T target)
        {
            MapDataToFields<T>(data, target);
            return MapDataToProperties<T>(data, target);
        }

        public static T MapDataToProperties<T>(this Dictionary<string, object> data, T target)
        {
            var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty | BindingFlags.GetProperty);

            for (var i = 0; i < properties.Length; i++)
            {
                var propertyInfo = properties[i];

                if (data.TryGetValue(propertyInfo.Name, out var value))
                {
                    MapValueToProperty(target, value, propertyInfo.PropertyType, (target,value) => propertyInfo.SetValue(target, value), (target) => propertyInfo.GetValue(target));
                }
            }

            return target;
        }

        public static T MapDataToFields<T>(this Dictionary<string, object> data, T target)
        {
            var fieldInfo = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < fieldInfo.Length; i++)
            {
                var field = fieldInfo[i];

                if (data.TryGetValue(field.Name, out var value))
                {
                    MapValueToProperty(target, value, field.FieldType, (target, value) => field.SetValue(target, value), (target) => field.GetValue(target));
                }
            }

            return target;
        }

        public static T MapValueToProperty<T>(T target, object value, Type propertyType, Action<object, object> setValue, Func<object, object> getValue)
        {
            if (propertyType.IsPrimitive || propertyType == typeof(string))
            {
                setValue(target, value);
            }
            else if (propertyType.IsClass)
            {
                MapValueToValueTypeProperty(target, value, propertyType, setValue, getValue);
            }
            else if (propertyType.IsArray)
            {
                MapValueToArrayProperty(target, value, propertyType, setValue, getValue);
            }

            return target;
        }

        public static T MapValueToArrayProperty<T>(T target, object value, Type propertyType, Action<object, object> setValue, Func<object, object> getValue)
        {
            if (value != null)
            {
                if (value.GetType().IsArray)
                {
                    setValue(target, value);
                }
                else if (value is IList)
                {
                    var listValue = value as IList;
                   
                    Array array = (Array)getValue(target);
                        
                    if (array == null)
                    {
                        array = (Array)Activator.CreateInstance(propertyType.GetElementType(), listValue.Count);

                        for (var i = 0; i < listValue.Count; i++)
                        {
                            array.SetValue(listValue[i], i);
                        }

                        setValue(target, array);
                    }
                    else
                    {
                        for (var i = 0; i < listValue.Count; i++)
                        {
                            array.SetValue(listValue[i], i);
                        }
                    }
                }
            }
            else
            {
                setValue(target, null);
            }

            return target;
        }

        public static T MapValueToValueTypeProperty<T>(T target, object value, Type propertyType, Action<object, object> setValue, Func<object, object> getValue)
        {
            if (value != null)
            {
                if (propertyType.IsAssignableFrom(typeof(IList)))
                {
                    if (value is IList)
                    {
                        setValue(target, value);
                    }
                }
                else if (value is IDictionary)
                {
                    var currentValue = getValue(target);

                    if (currentValue == null)
                    {
                        currentValue = Activator.CreateInstance(propertyType);
                        setValue(target, currentValue);
                    }

                    MapData((Dictionary<string, object>)value, currentValue);
                }
            }            
            else
            {
                setValue(target, null);
            }

            return target;
        }
    }
}