using System;
using UnityEngine;


public class LocationList : MonoBehaviour, ILocationProvider
{
    public enum Order
    {
        FirstToLast,
        LastToFirst,
        Random,
        // attempt to spread out the location order over the available locations
        Spread
    }

    public Vector3[] locations;

    public Order order;

    public bool useRelativeLocations = true;

    public float locationScale = 1.0f;

    public float randomRadius = 0f;

    private int _currentIndex = 0;

    private int _lastRandomIndex = -1;

    public void Start()
    {
        if (locations.Length == 0)
        {
            locations = new Vector3[] { transform.position };

            Debug.LogError("No locations provided");
        }
    }

    public Vector3 GetNextLocation() => TransformLocation(SelectNextLocation(), randomRadius);
    
    public Vector3[] GetLocations(Vector3[] result)
    {
        if (order == Order.Spread)
        {
            var idx = 0;

            // odd 
            if (result.Length % 2 > 0)
            {
                result[idx] = locations[0];
                idx = 1;
            }

            while (idx < result.Length)
            {
                result[idx] = locations[idx + 1];
                idx++;
            }
        }
        else
        {
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = GetNextLocation();
            }
        }

        return result;
    }

    private Vector3 SelectNextLocation()
    { 
        return order switch
        {
            Order.FirstToLast => GetLocationAscending(),
            Order.LastToFirst => GetLocationDescending(),
            Order.Random => GetRandomLocation(),
            // SelectNextLocation doesn't really have meaning when using spread, so simply use 
            // ascending.
            Order.Spread=> GetLocationAscending(),
            _ => throw new InvalidProgramException("unhandled order type: " + order),
        };
    }

    private Vector3 GetLocationAscending()
    {
        var result = locations[_currentIndex];

        _currentIndex = (_currentIndex + 1) % locations.Length;
        
        return result;
    }

    private Vector3 GetLocationDescending()
    {
        var result = locations[_currentIndex];

        _currentIndex--;

        if (_currentIndex < 0)
        {
            _currentIndex = locations.Length - 1;
        }

        return result;
    }

    private Vector3 GetRandomLocation()
    {
        // try to (soft) guarantee the randomization will not choose the same location twice
        if (locations.Length > 1)
        {
            for (int i = 0; i < 4; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, locations.Length);
                if (randomIndex != _lastRandomIndex)
                {
                    _lastRandomIndex = randomIndex;
                    break;
                }
            }
        } 
        else
        {
            _lastRandomIndex = 0;
        }

        return locations[_lastRandomIndex];
    }

    private Vector3 TransformLocation(in Vector3 location, float randomOffsetRadius)
    {
        Vector3 randomOffset = randomOffsetRadius > 0
                ? GetRandomRadius(randomOffsetRadius)
                : Vector3.zero;

        return useRelativeLocations
                    ? randomOffset + transform.position + locationScale * location
                    : randomOffset + locationScale * location;
    }

    private Vector3 GetRandomRadius(float randomOffsetRadius)
    {
        var randomLocation = UnityEngine.Random.insideUnitSphere * randomOffsetRadius;

        return new Vector3(randomLocation.x, randomLocation.y, 0);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = randomRadius <= 0 ? Color.black : Color.green;

        if (locations != null && locations.Length > 0)
        {
            for (var i = 0; i < locations.Length; i++)
            {
                Gizmos.DrawWireSphere(TransformLocation(locations[i], 0), Mathf.Max(randomRadius, 0.2f));
            }
        }
    }
}

