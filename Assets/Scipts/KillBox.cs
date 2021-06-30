using UnityEngine;

public class KillBox : MonoBehaviour
{
    public Bounds killBoxDimensions;

    private GameObjectMeta _metaInformation;

    private void Start()
    {
        _metaInformation = GetComponent<GameObjectMeta>();
    }

    void Update()
    {
        if (!killBoxDimensions.Contains(gameObject.transform.position))
        {
            if (_metaInformation != null)
            {
                _metaInformation.TryRelease();
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
