using UnityEngine;

public class KillBox : MonoBehaviour
{
    public Bounds killBoxDimensions;

    private PoolObject _metaInformation;

    private void Start()
    {
        _metaInformation = GetComponent<PoolObject>();
    }

    void Update()
    {
        if (!killBoxDimensions.Contains(gameObject.transform.position))
        {
            if (_metaInformation != null)
            {
                _metaInformation.Release();
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(killBoxDimensions.center, killBoxDimensions.extents * 2.0f);
    }
}
