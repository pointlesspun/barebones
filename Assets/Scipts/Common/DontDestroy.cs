using UnityEngine;

public class DontDestroy : MonoBehaviour
{
    public bool _destroySelfIfExists = false;

    private void Awake()
    {
        if (_destroySelfIfExists && GameObject.FindGameObjectsWithTag(gameObject.tag).Length > 1)
        {
            Destroy(gameObject);
        } else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

