using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameObjectMeta : MonoBehaviour
{
    public int poolId;
    public bool isReleased = false;
    public bool deferRelease = false;

    public void TryRelease()
    {
        if (deferRelease)
        {
            isReleased = true;
            gameObject.SetActive(false);
        }
        else
        {
            ObjectPoolCollection.instance.Release(this);
        }
    }
}

