
using UnityEngine;
using UnityEngine.SceneManagement;

using BareBones.Common.Messages;

namespace BareBones.Common.Behaviours
{
    public enum LifeCycle
    {
        Scene,
        Session,
        Application,
    }

    public class DontDestroy : MonoBehaviour, IMessageListener
    {
        public bool _destroySelfIfExists = false;

        public LifeCycle _lifeCycle = LifeCycle.Application;

        private IMessageBus _messageBus;
        private int _listenerHandle;

        public void Awake()
        {
            if (_destroySelfIfExists && GameObject.FindGameObjectsWithTag(gameObject.tag).Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void OnEnable()
        {
            if (_lifeCycle == LifeCycle.Scene || _lifeCycle == LifeCycle.Session)
            {
                if (_messageBus == null)
                {
                    _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
                }

                _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Scene);
            }
        }

        public void OnDisable()
        {
            if (_messageBus != null && _listenerHandle != -1)
            {
                _messageBus.Unsubscribe(_listenerHandle);
            }
        }

        public void HandleMessage(Message message)
        {
            if ((message.id == MessageIds.SceneEnded && _lifeCycle == LifeCycle.Scene)
                || message.id == MessageIds.SessionEnded)
            {
                // just clear the 'don't destroy' - the game object will be cleaned up when 
                // the scene unloads. 
                SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            }
        }
    }

}