using System;

using UnityEngine;
using UnityEngine.UI;

using BareBones.Common.Messages;

public class StartButton : MonoBehaviour, IGameMessageListener
{
    public string waitingForPlayersText = "Waiting for players...";
    public string startText = "Start !!!";

    private IGameMessageBus _eventBus;

    private int _playersRegistered = 0;

    private Button _button;
    private TMPro.TMP_Text _buttonText;

    public int GameMessageFlags => GameMessageIds.PlayerCanceled | GameMessageIds.PlayerJoined;

    void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IGameMessageBus>();

        _eventBus.AddListener(this);

        _button = GetComponent<Button>();

        _buttonText = transform.GetComponentInChildren<TMPro.TMP_Text>();

        if (_buttonText != null)
        {
            _buttonText.text = waitingForPlayersText;
        }
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
        // xxx this needs refinement
        _playersRegistered += evt.messageId == GameMessageIds.PlayerJoined ? 1 : -1;
        
        var allPlayersReady = _playersRegistered > 0;

        _button.interactable = allPlayersReady;

        if (_buttonText != null)
        {
            _buttonText.text = allPlayersReady
                ? startText
                : waitingForPlayersText;
        }
    }

    public void OnDisable()
    {
        _eventBus.RemoveListener(this);
    }
}

