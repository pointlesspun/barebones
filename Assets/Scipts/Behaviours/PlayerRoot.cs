using UnityEngine;
using UnityEngine.InputSystem;
using BareBones.Common.Messages;

public enum AgentControllerSource
{
    ControlledObject,
    FirstChild
}

public class PlayerRoot : MonoBehaviour, IGameMessageListener
{
    public string _playerName;
    public int _score;
    public int _lives;

    public PlayerInput _input;

    public AgentControllerSource _obtainControllerFrom = AgentControllerSource.ControlledObject;
    public GameObject _controlledObject;
    public int[] _deviceIds;

    private IGameMessageBus _messageBus;
    private AgentController _controller;

    private int _id;

    public int Id => _id;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Entity | GameMessageCategories.Scene;

    public bool IsAlive => _controller != null && _controller.gameObject.activeSelf;

    public GameMessageListenerState GameMessageListenerState { get; set; } = GameMessageListenerState.None;

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
            _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
        }

        _messageBus.AddListener(this);
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
            _messageBus.Send(GameMessageCategories.Player, GameMessageIds.PlayerCanceled, gameObject, Id);
        }      
    }

    public void HandleMessage(GameMessage message)
    {
        switch (message.messageCategory)
        {
            case GameMessageCategories.Entity:

                if (message.messageId == GameMessageIds.EntityDestroyed && message.sender == _controller.gameObject)
                {
                    _controller.gameObject.SetActive(false);
                }
                break;

            case GameMessageCategories.Scene:
                if (message.messageId == GameMessageIds.SceneStarted)
                {
                    gameObject.ActivateHierarchyTree(true);
                }
                break;
            default:
                Debug.LogWarning("PlayerRoot.HandleMessage, unhandled message category " + message.messageCategory);
                break;
        }
    }

    public void OnDisable()
    {
        if (_messageBus != null)
        {
            _messageBus.RemoveListener(this);
        }
    }
}
