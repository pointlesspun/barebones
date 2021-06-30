﻿
using UnityEngine;

public interface IWeapon
{
    void Fire();

    void Fire(in Vector3 localStartPosition);

    void Fire(Transform transformGroup, in Vector3 localStartPosition);
}

