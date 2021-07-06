using UnityEngine;

using BareBones.Common.Messages;

public class Hitpoints : MonoBehaviour
{
    public float baselineHitpoints = 1;

    public float currentHitpoints = 1;

    public bool isInvulnerable = false;

    public bool deferDestruction = false;

    private PoolObject _meta;

    private IGameMessageBus _eventBus;

    void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
        _meta = GetComponent<PoolObject>();
        currentHitpoints = baselineHitpoints;
    }

    public void OnHit(float damage)
    {
        if (!isInvulnerable)
        {
            currentHitpoints -= damage;

            if (currentHitpoints <= 0)
            {
                _eventBus.Send(GameMessageCategories.Entity, GameMessageIds.EntityDestroyed, gameObject, null);

                if (_meta != null)
                {
                    _meta.Release();
                } 
                else if (!deferDestruction) 
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}