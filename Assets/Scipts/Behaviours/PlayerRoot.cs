using UnityEngine;
using UnityEngine.InputSystem;
using BareBones.Common.Messages;

public enum AgentControllerSource
{
    ControlledObject,
    FirstChild
}

public class PlayerRoot : MonoBehaviour, IMessageListener
{
    public string _playerName;
    public int _score;
    public int _lives;

    public PlayerInput _input;

    public AgentControllerSource _obtainControllerFrom = AgentControllerSource.ControlledObject;
    public GameObject _controlledObject;
    public int[] _deviceIds;

    private IMessageBus _messageBus;
    private int _messageListenerHandle;
    private AgentController _controller;

    private int _id;

    public int Id => _id;

    public bool IsAlive => _controller != null && _controller.gameObject.activeSelf;

    public void OnEnable()
    {
        RegisterWithMessageBus();
    }

    public void Start()
    {
        SelectAgentController();
    }

    private void RegisterWithMessageBus()
    {
        if (_messageBus == null)
        {
            _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
        }

        _messageListenerHandle = _messageBus.Subscribe(this, MessageTopics.Entity);

        Debug.Assert(_messageListenerHandle > 0);
    }

    private void SelectAgentController()
    {
        switch (_obtainControllerFrom)
        {
            case AgentControllerSource.ControlledObject:
                if (_controlledObject != null)
                {
                    _controller = _controlledObject.GetComponent<AgentController>();
                }
                else
                {
                    Debug.LogWarning("PlayerRoot.SelectAgentController, cannot get agent controller from controlled object if controlled object is null.");
                }

                break;
            case AgentControllerSource.FirstChild:
                if (transform.childCount >= 0)
                {
                    _controller = transform.GetChild(0).gameObject.GetComponent<AgentController>();
                }
                else
                {
                    Debug.LogWarning("PlayerRoot.SelectAgentController, cannot get agent controller from children when no children were added.");
                }
                break;
            default:
                break;

        }

        if (_controller == null)
        {
            Debug.LogWarning("PlayerRoot.SelectAgentController no agent controller found in controller provider.");
        }
    }

    public PlayerRoot Initialize(int id, string name, PlayerInput input, int[] deviceIds)
    {
        _id = id;
        _playerName = name;
        _input = input;
        _deviceIds = deviceIds;

        return this;
    }

    public void ChangeFiringState(InputAction.CallbackContext context)
    {
        if (_controller != null && OwnsDevice(context.control.device))
        {
            _controller.ChangeFiringState(context);
        }
    }

    private bool OwnsDevice(InputDevice device)
    {
        for (var i = 0; i < _deviceIds.Length; i++)
        {
            if (device.deviceId == _deviceIds[i])
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateDirection(InputAction.CallbackContext context)
    {
        if (_controller != null && OwnsDevice(context.control.device))
        {
            _controller.UpdateDirection(context);
        }
    }

    public void OnCancel(InputAction.CallbackContext context )
    {
        // what happes depends on whether we're in the title screen or 
        // in game, so pass in on to the scene logic in charge
        
        if (this._input != null && context.performed && OwnsDevice(context.control.device))
        {
            _messageBus.Send(MessageTopics.Player, MessageIds.PlayerCanceled, gameObject, Id);
        }      
    }

    public void HandleMessage(Message message)
    {
        // was the controlled object destroyed ?
        if (message.id == MessageIds.EntityDestroyed && message.sender == _controller.gameObject)
        {
            _controller.gameObject.SetActive(false);
            _messageBus.Send(MessageTopics.Player, MessageIds.PlayerDied, gameObject, null);
        }
    }

    public void OnDisable()
    {
        if (_messageBus != null && _messageListenerHandle >= 0)
        {
            _messageBus.Unsubscribe(_messageListenerHandle);
        }
    }
}
