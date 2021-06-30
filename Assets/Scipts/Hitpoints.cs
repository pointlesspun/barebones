using UnityEngine;

public class Hitpoints : MonoBehaviour
{
    public float baselineHitpoints = 1;

    public float currentHitpoints = 1;

    public bool isInvulnerable = false;

    public bool deferDestruction = false;

    private GameObjectMeta _meta;

    private IEventBus _eventBus;

    void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IEventBus>();
        _meta = GetComponent<GameObjectMeta>();
        currentHitpoints = baselineHitpoints;
    }

    public void OnHit(float damage)
    {
        if (!isInvulnerable)
        {
            currentHitpoints -= damage;

            if (currentHitpoints <= 0)
            {
                _eventBus.Send(GameEventIds.EntityDestroyed, gameObject, null);

                if (_meta != null)
                {
                    _meta.TryRelease();
                } 
                else if (!deferDestruction) 
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
