using BareBones.Common.Messages;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LifeCycle
{  
    Scene,
    Session,
    Application,
}

public class DontDestroy : MonoBehaviour, IGameMessageListener
{
    public bool _destroySelfIfExists = false;

    public LifeCycle _lifeCycle = LifeCycle.Application;

    private IGameMessageBus _messageBus;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Scene;

    
    void Awake()
    {
        if (_destroySelfIfExists && GameObject.FindGameObjectsWithTag(gameObject.tag).Length > 1)
        {
            Destroy(gameObject);
        } else
        {
            DontDestroyOnLoad(gameObject);

            if (_lifeCycle == LifeCycle.Scene || _lifeCycle == LifeCycle.Session)
            {
                if (_messageBus == null)
                {
                    _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();

                    _messageBus.AddListener(this);
                }
            }
        }
    }

    void OnDestroy()
    {
        if (_messageBus != null)
        {
            _messageBus.RemoveListener(this);
        }
    }

    public void HandleMessage(GameMessage message)
    {
        if ((message.messageId == GameMessageIds.SceneEnded && _lifeCycle == LifeCycle.Scene)
            || message.messageId == GameMessageIds.SessionEnded)
        {
            // just clear the 'don't destroy' - the game object will be cleaned up when 
            // the scene unloads. 
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        }
    }
}

