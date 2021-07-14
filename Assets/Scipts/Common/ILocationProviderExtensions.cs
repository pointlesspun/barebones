using System.Collections.Generic;
using UnityEngine;

namespace BareBones.Common
{
    public static class ILocationProviderExtensions
    {
        public static void AssignLocations<T>(this ILocationProvider provider, IEnumerable<T> components, int count) where T : Component
        {
            var startLocations = provider.GetLocations(new Vector3[count]);
            var idx = 0;

            foreach (var component in components)
            {
                component.transform.position = startLocations[idx];
                idx++;
            }
        }
    }
}
