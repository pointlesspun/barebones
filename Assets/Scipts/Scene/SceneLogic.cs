using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using BareBones.Common.Messages;

public class SceneLogic : MonoBehaviour, IGameMessageListener, ITimeoutCallback
{
    public string _titleSceneName;
    /**
     * Time after the game over has been signalled before the next scene is loaded
     */
    public float _gameOverTimeout = 1.0f;

    private IGameMessageBus _messageBus;
    private IPlayerRegistry _playerRegistry;
    private ITimeService _timeService;

    private int _timeOutHandle = -1;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Entity;

    private void Start()
    {
        if (_messageBus == null)
        {
            _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
            _messageBus.AddListener(this);
            _messageBus.Send(GameMessageCategories.Scene, GameMessageIds.SceneStarted, gameObject, null);
        }

        _playerRegistry = ResourceLocator._instance.Resolve<IPlayerRegistry>();
        _timeService = ResourceLocator._instance.Resolve<ITimeService>();
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

    public void HandleMessage(GameMessage message)
    {
        if (message.messageCategory == GameMessageCategories.Entity
            && message.messageId == GameMessageIds.EntityDestroyed
            && message.sender != null
            && message.sender.CompareTag("Player")
            && !_playerRegistry.Any(player => player.IsAlive))
        {
            // send game over
            _messageBus.Send(GameMessageCategories.Scene, GameMessageIds.SessionEnded, gameObject, null);

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
        _messageBus.RemoveListener(this);

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
