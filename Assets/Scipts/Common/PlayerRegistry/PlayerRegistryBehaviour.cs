using BareBones.Common.Messages;
using UnityEngine;


public class PlayerRegistryBehaviour : MonoBehaviour, IMessageListener
{
    public int maxPlayers = 3;

    public int initialActivePlayers = 0;

    private PlayerRegistry _registry;
    private IObjectPoolCollection _objectPool;
    private ILocationProvider _startLocationProvider;
    private IMessageBus _messageBus;
    private int _listenerHandle;

    public void Awake()
    {
        if (_registry == null && !ResourceLocator._instance.Contains<IPlayerRegistry>())
        {
            _registry = ResourceLocator._instance.Register<IPlayerRegistry, PlayerRegistry>(maxPlayers);
        }
    }

    public void OnEnable()
    {
        if (_messageBus == null)
        {
            _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
        }

        _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Scene);
    }

    public void OnDisable()
    {
        if (_messageBus != null && _listenerHandle != -1)
        {
            _messageBus.Unsubscribe(_listenerHandle);
        }
    }


    public void Start()
    {
        _objectPool = ResourceLocator._instance.Resolve<IObjectPoolCollection>();

        for (var i = 0; i < initialActivePlayers; i++)
        {
            var playerPoolObject = _objectPool.Obtain((int) PoolIdEnum.Players);
            _registry.RegisterPlayer(playerPoolObject.GetComponent<PlayerRoot>());
            playerPoolObject.gameObject.ActivateHierarchyTree(true);
        }
    }

    public void OnDestroy()
    {
        if (_registry != null)
        {
            ResourceLocator._instance.Deregister<IPlayerRegistry>();
            _registry = null;
        }
    }

    public void HandleMessage(Message message)
    {
        if (message.id == MessageIds.SceneStarted)
        {
            // scene has begun, activate the players put them in their starting location
            foreach (var root in _registry)
            {
                root.gameObject.ActivateHierarchyTree(true);
            }

            _startLocationProvider = GetComponent<ILocationProvider>();

            if (_startLocationProvider != null)
            {
                _startLocationProvider.AssignLocations(_registry, _registry.PlayerCount);
            }
            else
            {
                Debug.LogWarning("No location provider in player registry, players will be put at their default position.");
            }
        }
    }
}

