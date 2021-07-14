
using UnityEngine;

namespace BareBones.Common.Behaviours
{
    public interface IWeapon
    {
        void Fire();

        void Fire(in Vector3 localStartPosition);

        void Fire(Transform transformGroup, in Vector3 localStartPosition);
    }
}

