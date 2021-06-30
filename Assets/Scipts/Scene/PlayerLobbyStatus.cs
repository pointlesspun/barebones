
using UnityEngine;

public class PlayerLobbyStatus : MonoBehaviour, IGameEventListener
{
    private IEventBus _eventBus;

    public int EventFlags => GameEventIds.PlayerCanceled;

    public int GameEventFlags => GameEventIds.PlayerJoined | GameEventIds.PlayerCanceled;


    void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IEventBus>();       
        _eventBus.AddListener(this);
    }

    void OnEnable()
    {
        if (_eventBus != null)
        {
            _eventBus.AddListener(this);
        }
    }

    public void HandleGameEvent(GameEvent evt)
    {
        var playerIndex = (int)evt.payload;

        if (playerIndex >= 0 && transform.childCount > playerIndex)
        {
            transform.GetChild(playerIndex).gameObject.SetActive(evt.eventId == GameEventIds.PlayerJoined);
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
