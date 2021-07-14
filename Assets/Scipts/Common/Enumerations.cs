
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.Utilities;

namespace BareBones.Common
{
    public static class Enumerations
    {
        public static int GetAvailableSlot<T>(this T[] array) where T : class
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool Any(this int[] array, int value)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Any<T>(this T[] array, T value) where T : class
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Any<T>(this ReadOnlyArray<T> array, T value) where T : class
        {
            for (var i = 0; i < array.Count; i++)
            {
                if (array[i] == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<TProperty> Select<TValue, TProperty>(
            this ReadOnlyArray<TValue> array,
            Func<TValue, TProperty> predicate
        ) where TValue : class where TProperty : class
        {
            var result = new List<TProperty>();

            for (var i = 0; i < array.Count; i++)
            {
                if (array[i] != null)
                {
                    TProperty property = predicate(array[i]);

                    result.Add(property);
                }
            }

            return result;
        }

        public static bool Any<T>(this T[] array, Func<T, bool> predicate)
        {
            for (var i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Any<T>(this IEnumerable<T> enumeration, Func<T, bool> predicate)
        {
            foreach (var value in enumeration)
            {
                if (predicate(value))
                {
                    return true;
                }
            }

            return false;
        }

        public static T[] CreateArray<T>(int count)
        {
            var result = new T[count];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = Activator.CreateInstance<T>();
            }

            return result;
        }

        public static T[] CreateArray<T>(int count, Func<int, T> factoryMethod)
        {
            var result = new T[count];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = factoryMethod(i);
            }

            return result;
        }

        public static T[] SetForEach<T>(this T[] array, Func<T, T> function)
        {
            for (var i = 0; i < array.Length; i++)
            {
                array[i] = function(array[i]);
            }

            return array;
        }

        public static T[] ForEach<T>(this T[] array, Action<T> action)
        {
            for (var i = 0; i < array.Length; i++)
            {
                action(array[i]);
            }

            return array;
        }
    }
}