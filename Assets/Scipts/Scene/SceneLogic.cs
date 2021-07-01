using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

using BareBones.Common.Messages;

public class SceneLogic : MonoBehaviour
{
    public string _titleSceneName;
    public PoolObject[] _activePlayers;

    private IGameMessageBus _messageBus;

    private void Start()
    {
        _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
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
                _activePlayers = new PoolObject[playerObjects.Length];

                for (var i = 0; i < playerObjects.Length; i++)
                {
                    _activePlayers[i] = playerObjects[i].GetComponent<PoolObject>();
                }
            }
        }

        // xxx use listeners
        if (_messageBus != null && _messageBus.ReadBufferLength > 0)
        {
            for (var i = 0; i < _messageBus.ReadBufferLength; i++ )
            {
                var message = _messageBus.Read(i);
                if (message.messageCategory == GameMessageCategories.Entity
                    && message.messageId == GameMessageIds.EntityDestroyed
                    && message.sender != null
                    && message.sender.CompareTag("Player")
                    && GetLivingPlayerCount(_activePlayers) == 0)
                {
                    
                    StartCoroutine(LoadScene(_titleSceneName));
                }
            }
        }   
    }

    public int GetLivingPlayerCount(PoolObject[] players)
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
