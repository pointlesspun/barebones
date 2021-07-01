
using UnityEngine;

using BareBones.Common.Messages;
 
public class PlayerLobbyStatus : MonoBehaviour, IGameMessageListener
{
    private IGameMessageBus _eventBus;

    public int EventFlags => GameMessageIds.PlayerCanceled;

    public int GameMessageFlags => GameMessageIds.PlayerJoined | GameMessageIds.PlayerCanceled;


    void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IGameMessageBus>();       
        _eventBus.AddListener(this);
    }

    void OnEnable()
    {
        if (_eventBus != null)
        {
            _eventBus.AddListener(this);
        }
    }

    public void HandleMessage(GameMessage evt)
    {
        var playerIndex = (int)evt.payload;

        if (playerIndex >= 0 && transform.childCount > playerIndex)
        {
            transform.GetChild(playerIndex).gameObject.SetActive(evt.messageId == GameMessageIds.PlayerJoined);
        }
        else
        {
            Debug.LogWarning("Player index (" + playerIndex + ") is outside the range of children the PlayerLobbyStatus can affect. Make sure there the number of children matches the max number of players.");
        }
    }

    public void OnDisable()
    {
        _eventBus.RemoveListener(this);
    }
}
