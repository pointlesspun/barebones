using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class PlayerRoot : MonoBehaviour
{
    public string _playerName;
    public int _score;
    public int _lives;

    public PlayerInput _input;
    public AgentController _controlledObject;
    public int[] _deviceIds;

    private IEventBus _eventBus;

    public int Id => _input.playerIndex;

    public void Start()
    {
        _eventBus = ResourceLocator._instance.Resolve<IEventBus>();
    }

    public PlayerRoot Initialize(string name, PlayerInput input, int[] deviceIds, AgentController controlledObject = null, int score = 0)
    {
        _playerName = name;
        _input = input;
        _score = score;
        _controlledObject = controlledObject;
        _deviceIds = deviceIds;

        return this;
    }

    public void ChangeFiringState(InputAction.CallbackContext context)
    {
        if (_controlledObject != null)
        {
            _controlledObject.ChangeFiringState(context);
        }
    }

    public void UpdateDirection(InputAction.CallbackContext context)
    {
        if (_controlledObject != null)
        {
            _controlledObject.UpdateDirection(context);
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

