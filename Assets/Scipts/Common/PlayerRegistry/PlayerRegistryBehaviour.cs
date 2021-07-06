using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerRegistryBehaviour : MonoBehaviour
{
    public int maxPlayers = 3;

    public int initialActivePlayers = 0;

    private PlayerRegistry _registry;
    private IObjectPoolCollection _objectPool;

    public void Awake()
    {
        if (_registry == null && !ResourceLocator._instance.Contains<IPlayerRegistry>())
        {
            _registry = ResourceLocator._instance.Register<IPlayerRegistry, PlayerRegistry>(maxPlayers);
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
}

