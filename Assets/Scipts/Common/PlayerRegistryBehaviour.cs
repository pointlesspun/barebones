using UnityEngine;
using UnityEngine.SceneManagement;

public enum ResetPlayerRegistryCondition
{
    Never,
    WhenLoadingScene
}

public class PlayerRegistryBehaviour : MonoBehaviour
{
    public int maxPlayers = 3;

    public ResetPlayerRegistryCondition reset = ResetPlayerRegistryCondition.Never;
    public string resetScene = "";


    private PlayerRegistry _registry;

    public void Awake()
    {
        if (_registry == null && !ResourceLocator._instance.Contains<IPlayerRegistry>())
        {
            _registry = ResourceLocator._instance.Register<IPlayerRegistry, PlayerRegistry>(maxPlayers);

            if (_registry != null && reset != ResetPlayerRegistryCondition.Never)
            {
                if (reset == ResetPlayerRegistryCondition.WhenLoadingScene)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                }
            }
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_registry.PlayerCount > 0 && scene.name == resetScene)
        {
            _registry.Reset();
        }
    }
}

