using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using BareBones.Common.Messages;
using UnityEngine.InputSystem.Users;

public enum PlayerParentTransform
{
    None,
    GameObject,
    Path,
    Tag,
    Tag_Path
}

public class LocalPlayerLobby : MonoBehaviour, IGameMessageListener
{
    public GameObject playerPrefab;

    public PlayerParentTransform playerParentTransform;
    public GameObject parentObject;
    public string parentString;

    public string[] joinActionPaths = new string[] {
        "<Gamepad>/buttonSouth",
        "<Mouse>/leftButton",
        "<Keyboard>/space"
    };

    private InputAction _action;

    private IPlayerRegistry _registry;

    private IGameMessageBus _messageBus;

    public GameMessageCategories CategoryFlags => GameMessageCategories.Player;

    public void Start()
    {
        _registry = ResourceLocator._instance.Resolve<IPlayerRegistry>();
        _messageBus = ResourceLocator._instance.Resolve<IGameMessageBus>();
        
        _messageBus.AddListener(this);
        
        _action = new InputAction();

        foreach (var path in joinActionPaths)
        {
            _action.AddBinding(path);
        }

        _action.canceled += HandleRegisterPlayer;
        _action.Enable();
    }

    void OnEnable()
    {
        if (_messageBus != null)
        {
            _messageBus.AddListener(this);
        }
    }

    public void OnDisable()
    {
        if (_messageBus != null)
        {
            _messageBus.RemoveListener(this);
        }

        _action.Dispose();
    }

    public void HandleRegisterPlayer(InputAction.CallbackContext context)
    {
        if (!EventSystem.current.IsPointerOverGameObject()
            && _registry.AvailableSlots > 0 
            && !HasPlayerRegistered(context.action.activeControl.device))
        {
            var newPlayerRoot = NewInstance(context.control.device);

            _registry.RegisterPlayer(newPlayerRoot);

            switch (playerParentTransform)
            {
                case PlayerParentTransform.GameObject:
                    newPlayerRoot.gameObject.transform.parent = parentObject.transform;
                    break;
                case PlayerParentTransform.Path:
                    newPlayerRoot.gameObject.transform.parent = transform.Find(parentString);
                    break;
                case PlayerParentTransform.Tag:
                    AttachToParentByTag(newPlayerRoot.gameObject, parentString);
                    break;
                case PlayerParentTransform.Tag_Path:
                    AttachToParentByTagPath(newPlayerRoot.gameObject, parentString);
                    break;
                case PlayerParentTransform.None:
                default:
                    break;

            }

            _messageBus.Send(GameMessageCategories.Player, GameMessageIds.PlayerJoined, gameObject, newPlayerRoot.Id);
        }
    }

    private void AttachToParentByTagPath(GameObject obj, string str)
    {
        var pathSeparatorIndex = parentString.IndexOf('/');
        if (pathSeparatorIndex >= 0)
        {
            var tag = str.Substring(0, pathSeparatorIndex);

            var parent = FindObjectWithTag(tag);

            if (parent != null)
            {
                var path = parentString.Substring(pathSeparatorIndex + 1);
                obj.transform.parent = parent.transform.Find(path);
            }

        }
        else
        {
            AttachToParentByTag(obj, parentString);
        }
    }

    private void AttachToParentByTag(GameObject obj, string tag)
    {
        var parent = FindObjectWithTag(tag);

        if (parent != null)
        {
            obj.transform.parent = parent.transform;
        }
    }

    private GameObject FindObjectWithTag(string tag)
    {
        var taggedObjects = GameObject.FindGameObjectsWithTag(tag);

        if (taggedObjects.Length == 1)
        {
            return taggedObjects[0];
        }
        else if (taggedObjects == null || taggedObjects.Length == 0 )
        {
            Debug.LogWarning("No objects with tag " + tag + " exist.");
        }
        else 
        {
            Debug.LogWarning("More than one object with tag " + tag + " exists.");
        }

        return null;
    }

    public void HandleMessage(GameMessage message)
    {
        if (message.messageId == GameMessageIds.PlayerCanceled)
        {          
            _registry.DeregisterPlayer((int)message.payload);

            message.sender.GetComponent<PoolObject>().TryRelease();
        }
    }

    private PlayerRoot NewInstance(InputDevice device)
    {
        var (devices, controlScheme, deviceIds) = CreateInputConfiguration(device);

        var poolObject = ObjectPoolCollection.instance.Obtain((int)PoolIdEnum.Players);

        var root = poolObject.GetComponent<PlayerRoot>();
        var input = poolObject.GetComponent<PlayerInput>();
        var id = _registry.GetAvailableIndex();

        input.user.UnpairDevices();
        devices.ForEach(dev => InputUser.PerformPairingWithDevice(dev, user: input.user));

        input.defaultControlScheme = controlScheme;
        input.SwitchCurrentControlScheme(controlScheme);
        
        var name = playerPrefab.name + " " + id;

        input.gameObject.name = name;

        return root.Initialize(
            id,
            name, 
            input, 
            deviceIds
        );
    }

    private (InputDevice[] devices, string controlScheme, int[] deviceIds)
        CreateInputConfiguration(InputDevice device)
    {
        if (device is Gamepad)
        {
            return (
                new InputDevice[] { device },
                "Gamepad",
                new int[] { device.deviceId }
            );
        }
        else if (device is Keyboard || device is Mouse)
        {
            return (
                new InputDevice[] { Keyboard.current, Mouse.current },
                "Keyboard&Mouse",
                new int[] { Keyboard.current.deviceId, Mouse.current.deviceId }
            );
        }

        throw new NotImplementedException("Unknown device " + device.name);
    }

    private bool HasPlayerRegistered(InputDevice device)
    {
        return _registry.HasPlayerRegistered( root => root._deviceIds.Any(device.deviceId));
    }
}
