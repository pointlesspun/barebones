using UnityEngine;

public class PoolObject : MonoBehaviour
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

