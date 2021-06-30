
using UnityEngine;

public class TitleScreenLogic : MonoBehaviour, IGameEventListener
{
    public string nextScene;

    public GameObject[] playerJoinedText;

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
        switch (evt.eventId)
        {
            case GameEventIds.PlayerCanceled:
                playerJoinedText[(int)evt.payload].SetActive(false);
                break;
            case GameEventIds.PlayerJoined:
                playerJoinedText[(int)evt.payload].SetActive(true);
                break;
        }
    }

    public void OnDisable()
    {
        _eventBus.RemoveListener(this);
    }
}
