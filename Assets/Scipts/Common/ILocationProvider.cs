
using UnityEngine;

namespace BareBones.Common
{
    public interface ILocationProvider
    {
        Vector3 GetNextLocation();

        Vector3[] GetLocations(Vector3[] result);
    }
}
