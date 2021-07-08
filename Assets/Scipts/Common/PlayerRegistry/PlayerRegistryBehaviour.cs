using BareBones.Common.Messages;
using UnityEngine;


public class PlayerRegistryBehaviour : MonoBehaviour, IGameMessageListener
{
    public int maxPlayers = 3;

    public int initialActivePlayers = 0;

    private PlayerRegistry _registry;
    private IObjectPoolCollection _objectPool;
    private ILocationProvider _startLocationProvider;
    private IGameMessageBus _messageBus;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Scene;

    public GameMessageListenerState GameMessageListenerState { get; set; } = GameMessageListenerState.None;

    public void Awake()
    {
        if (_registry == null && !ResourceLocator._instance.Contains<IPlayerRegistry>())
        {
            _registry = ResourceLocator._instance.Register<IPlayerRegistry, PlayerRegistry>(maxPlayers);
        }
    }

    public void Start()
    {
        if (_messageBus == null)
        {
            _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
            _messageBus.AddListener(this);
        }

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

        if (_messageBus != null)
        {
            _messageBus.RemoveListener(this);
            _messageBus = null;
        }
    }

    public void HandleMessage(GameMessage message)
    {
        if (message.messageId == GameMessageIds.SceneStarted)
        {
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

