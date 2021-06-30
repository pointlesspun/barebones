using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLogic : MonoBehaviour
{
    public string _titleSceneName;
    public GameObjectMeta[] _activePlayers;

    private IEventBus _eventBus;

    private void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IEventBus>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_activePlayers == null || _activePlayers.Length == 0)
        {
            // need to wait until players have been spawned
            var playerObjects = GameObject.FindGameObjectsWithTag("Player");

            if (playerObjects.Length > 0)
            {
                _activePlayers = new GameObjectMeta[playerObjects.Length];

                for (var i = 0; i < playerObjects.Length; i++)
                {
                    _activePlayers[i] = playerObjects[i].GetComponent<GameObjectMeta>();
                }
            }
        }

        if (_eventBus.ReadBufferLength > 0)
        {
            for (var i = 0; i < _eventBus.ReadBufferLength; i++ )
            {
                var evt = _eventBus.Read(i);
                if (evt.eventId == GameEventIds.EntityDestroyed
                    && evt.sender != null
                    && evt.sender.CompareTag("Player")
                    && GetLivingPlayerCount(_activePlayers) == 0)
                {
                    
                    StartCoroutine(LoadScene(_titleSceneName));
                }
            }
        }   
    }

    public int GetLivingPlayerCount(GameObjectMeta[] players)
    {
        var count = 0;
        for (var i = 0; i < players.Length; i++)
        {
            if (!players[i].isReleased)
            {
                count++;
            }
        }
        return count;
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
}
