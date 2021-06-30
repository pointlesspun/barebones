using UnityEngine;

public class EventBusBehaviour : MonoBehaviour
{
    private EventBus _eventBus;
    private ResourceLocator _locator;

    public void Awake()
    {
        _locator = ResourceLocator._instance;
        
        if (_eventBus == null && !_locator.Contains<IEventBus>())
        {
            _eventBus = _locator.Register<IEventBus, EventBus>();
        }
    }

    public void Update()
    {
        if (_eventBus != null)
        {
            _eventBus.Update();
        }
    }
    
    public void OnDestroy()
    {
        if (_eventBus != null)
        {
            _eventBus.Clear();
            _locator.Deregister<IEventBus>(_eventBus);
            _eventBus = null;
        }
    }
}
