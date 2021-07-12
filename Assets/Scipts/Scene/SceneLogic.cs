using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using BareBones.Common.Messages;

public class SceneLogic : MonoBehaviour, IMessageListener, ITimeoutCallback
{
    public string _titleSceneName;
    /**
     * Time after the game over has been signalled before the next scene is loaded
     */
    public float _gameOverTimeout = 1.0f;

    private IPlayerRegistry _playerRegistry;
    private ITimeService _timeService;

    private int _timeOutHandle = -1;

    private IMessageBus _messageBus;
    private int _listenerHandle;

    private void Start()
    {
        _messageBus.Send(MessageTopics.Scene, MessageIds.SceneStarted, gameObject, null);
        _playerRegistry = ResourceLocator._instance.Resolve<IPlayerRegistry>();
        _timeService = ResourceLocator._instance.Resolve<ITimeService>();
    }

    public void OnEnable()
    {
        if (_messageBus == null)
        {
            _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
        }

        _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Entity);
    }

    public void OnDisable()
    {
        if (_messageBus != null && _listenerHandle != -1)
        {
            _messageBus.Unsubscribe(_listenerHandle);
        }
    }

    IEnumerator LoadScene(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    public void HandleMessage(Message message)
    {
        if (message.id == MessageIds.EntityDestroyed
            && message.sender != null
            && ((GameObject)message.sender).CompareTag("Player")
            && !_playerRegistry.Any(player => player.IsAlive))
        {
            // send game over
            _messageBus.Send(MessageTopics.Scene, MessageIds.SessionEnded, gameObject, null);

            _timeOutHandle = _timeService.SetTimeout(this, _gameOverTimeout);

            if (_timeOutHandle < 0 || _gameOverTimeout <= 0)
            {
                Debug.LogWarning("No more timeout handles available or _gameOverTimeOut is equal or less than zero, loading next scene now... ");
                StartCoroutine(LoadScene(_titleSceneName));
            }
        }
    }

    public void OnDestroy()
    {
        if (_timeOutHandle >= 0 && _timeService != null)
        {
            _timeService.Cancel(_timeOutHandle);
            _timeOutHandle = -1;
        }
    }

    public void OnTimeout(int handle)
    {
        _timeOutHandle = -1;
        StartCoroutine(LoadScene(_titleSceneName));
    }
}
