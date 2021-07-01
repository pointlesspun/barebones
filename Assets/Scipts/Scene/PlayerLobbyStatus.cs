
using UnityEngine;

using BareBones.Common.Messages;
 
public class PlayerLobbyStatus : MonoBehaviour, IGameMessageListener
{
    private IGameMessageBus _messageBus;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Player;


    void Start()
    {
        _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();       
        _messageBus.AddListener(this);
    }

    void OnEnable()
    {
        if (_messageBus != null)
        {
            _messageBus.AddListener(this);
        }
    }

    public void HandleMessage(GameMessage message)
    {
        var playerIndex = (int)message.payload;

        if (playerIndex >= 0 && transform.childCount > playerIndex)
        {
            transform.GetChild(playerIndex).gameObject.SetActive(message.messageId == GameMessageIds.PlayerJoined);
        }
        else
        {
            Debug.LogWarning("Player index (" + playerIndex + ") is outside the range of children the PlayerLobbyStatus can affect. Make sure there the number of children matches the max number of players.");
        }
    }

    public void OnDisable()
    {
        _messageBus.RemoveListener(this);
    }
}
