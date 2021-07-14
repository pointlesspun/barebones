using UnityEngine;

namespace BareBones.Common.Behaviours
{
    public static class PhysicsControl
    {
        public static Vector3 ClampVelocity(in Vector3 currentVelocity, float maxVelocity)
        {
            return currentVelocity.normalized * Mathf.Clamp(currentVelocity.magnitude, -maxVelocity, maxVelocity);
        }

        public static Vector3 ClampVelocity(
            in Vector3 currentVelocity,
            float velocityDampening,
            float maxVelocity)
        {
            return currentVelocity.normalized
                    * Mathf.Clamp(currentVelocity.magnitude, -maxVelocity, maxVelocity)
                    * velocityDampening;
        }


        public static Vector3 ClampPosition(in Vector3 position, in Bounds bounds)
        {
            return new Vector3(
                Mathf.Clamp(position.x, bounds.min.x, bounds.max.x),
                Mathf.Clamp(position.y, bounds.min.y, bounds.max.y),
                Mathf.Clamp(position.z, bounds.min.z, bounds.max.z)
            );
        }
    }

}