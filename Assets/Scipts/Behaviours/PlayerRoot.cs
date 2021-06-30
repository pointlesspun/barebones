using UnityEngine;
using UnityEngine.InputSystem;

public enum AgentControllerProvider
{
    ControlledObject,
    FirstChild
}

public class PlayerRoot : MonoBehaviour
{
    public string _playerName;
    public int _score;
    public int _lives;

    public PlayerInput _input;

    public AgentControllerProvider _obtainControllerFrom = AgentControllerProvider.ControlledObject;
    public GameObject _controlledObject;
    public int[] _deviceIds;

    private IEventBus _eventBus;
    private AgentController _controller;

    public int Id => _input.playerIndex;

    public void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IEventBus>();

        SelectAgentController();
    }

    private void SelectAgentController()
    {
        switch (_obtainControllerFrom)
        {
            case AgentControllerProvider.ControlledObject:
                if (_controlledObject != null)
                {
                    _controller = _controlledObject.GetComponent<AgentController>();
                }
                else
                {
                    Debug.LogWarning("PlayerRoot.SelectAgentController, cannot get agent controller from controlled object if controlled object is null.");
                }

                break;
            case AgentControllerProvider.FirstChild:
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
    public PlayerRoot Initialize(string name, PlayerInput input, int[] deviceIds, AgentController controlledObject = null, int score = 0)
    {
        _playerName = name;
        _input = input;
        _score = score;
        _controller = controlledObject;
        _deviceIds = deviceIds;

        return this;
    }

    public void ChangeFiringState(InputAction.CallbackContext context)
    {
        if (_controller != null)
        {
            _controller.ChangeFiringState(context);
        }
    }

    public void UpdateDirection(InputAction.CallbackContext context)
    {
        if (_controller != null)
        {
            _controller.UpdateDirection(context);
        }
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        // what happes depends on whether we're in the title screen or 
        // in game, so pass in on to the scene logic in charge
        
        if (this._input != null && context.performed)
        {
            _eventBus.Send(GameEventIds.PlayerCanceled, gameObject, Id);
        }      
    }
}

