
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
            var properties = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance );

            for (var i = 0; i < properties.Length; i++)
            {
                var propertyInfo = properties[i];

                if (propertyInfo.GetGetMethod() != null 
                    && propertyInfo.GetSetMethod() != null
                        && data.TryGetValue(propertyInfo.Name, out var value))
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
            else if (propertyType.IsArray)
            {
                MapValueToArrayProperty(target, value, propertyType, setValue, getValue);
            }
            else if (propertyType.IsClass)
            {
                MapValueToClassProperty(target, value, propertyType, setValue, getValue);
            }
            
            return target;
        }

        public static T MapValueToArrayProperty<T>(T target, object value, Type propertyType, Action<object, object> setValue, Func<object, object> getValue)
        {
            if (value != null)
            {
                if (value.GetType().IsArray)
                {
                    setValue(target, DeepCopyArray((Array)value));
                }
                else if (value is IList)
                {
                    var listValue = value as IList;

                    Array array = Array.CreateInstance(propertyType.GetElementType(), listValue.Count);

                    for (var i = 0; i < listValue.Count; i++)
                    {
                        array.SetValue(DeepCopy(listValue[i]), i);
                    }

                    setValue(target, array);
                }
                else throw new ArgumentException("Cannot map value of type " + value.GetType() + " to an array");
            }
            else
            {
                setValue(target, null);
            }

            return target;
        }

        public static T MapValueToClassProperty<T>(T target, object value, Type propertyType, Action<object, object> setValue, Func<object, object> getValue)
        {
            if (value != null)
            {
                if (propertyType.IsAssignableFrom(typeof(IList)))
                {
                    if (value is IList)
                    {
                        setValue(target, DeepCopyList(value as IList));
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

        public static T DeepCopy<T>(this T source)
        {
            if (source == null)
            {
                return default;
            }

            var type = source.GetType();

            if (type.IsPrimitive || type == typeof(string))
            {
                return source;
            }
            else if (type.IsArray)
            {
                return (T) ((object)DeepCopyArray(source as Array));
            }
            else if (source is IList)
            {
                return (T) DeepCopyList(source as IList);
            }
            else if (source is IDictionary)
            {
                return (T)DeepCopyDictionary(source as IDictionary);
            }
            else if (type.IsClass)
            {
                var target = Activator.CreateInstance(type);
                DeepCopyFields(source, target);
                DeepCopyProperties(source, target);
                return (T) target;
            }
            else throw new NotImplementedException("Cannot create a deep copy of value " + source.GetType() + ".");
        }

        public static IList DeepCopyList(this IList source)
        {
            var listType = source.GetType();

            IList result = null;

            if (listType.IsGenericType)
            {
                var args = listType.GetGenericArguments();
                var targetType = typeof(List<>).MakeGenericType(args);
                result = (IList)Activator.CreateInstance(targetType);
            }
            else
            {
                result = (IList)Activator.CreateInstance(listType);
            }

            for (var i = 0; i < source.Count; i++)
            {
                result.Add(DeepCopy(source[i]));
            }

            return result;
        }

        public static IDictionary DeepCopyDictionary(this IDictionary source)
        {
            var dictionaryType = source.GetType();

            IDictionary result;

            if (dictionaryType.IsGenericType)
            {
                var args = dictionaryType.GetGenericArguments();
                var targetType = typeof(Dictionary<,>).MakeGenericType(args);
                result = (IDictionary)Activator.CreateInstance(targetType);
            }
            else
            {
                result = (IDictionary)Activator.CreateInstance(dictionaryType);
            }

            foreach (var key in source.Keys)
            {
                var keyType = key.GetType();
                if (keyType.IsPrimitive || keyType == typeof(string))
                {
                    result.Add(key, DeepCopy(source[key]));
                }
                else throw new NotImplementedException("Cannot make a deep copy of dictionaries with anything but primitive types.");
            }

            return result;
        }

        public static Array DeepCopyArray(this Array source)
        {
            if (source != null)
            {
                var elementType = source.GetType().GetElementType();
                var destination = Array.CreateInstance(elementType, source.Length);

                if (source.Length > 0)
                {
                    if (elementType.IsPrimitive || elementType == typeof(string))
                    {
                        Array.Copy(source, destination, source.Length);
                    }
                    else
                    {
                        for (var i = 0; i < source.Length; i++)
                        {
                            var objCopy = source.GetValue(i);
                            destination.SetValue(objCopy, i);
                        }
                    }
                }

                return destination;
            }
            
            return default;
        }

        public static object DeepCopyFields(object source, object target)
        {
            var fieldInfo = target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < fieldInfo.Length; i++)
            {
                var field = fieldInfo[i];
                field.SetValue(target, DeepCopy(field.GetValue(source)));               
            }

            return target;
        }

        public static object DeepCopyProperties(object source, object target)
        {
            var propertyInfo = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (var i = 0; i < propertyInfo.Length; i++)
            {
                var property = propertyInfo[i];

                if (property.GetGetMethod() != null && property.GetSetMethod() != null)
                {
                    property.SetValue(target, DeepCopy(property.GetValue(source)));
                }
            }

            return target;
        }
    }
}