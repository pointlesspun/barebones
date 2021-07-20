using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

using UnityEngine.InputSystem.Users;

using BareBones.Common;
using BareBones.Common.Behaviours;
using BareBones.Services.Messages;
using BareBones.Services.ObjectPool;
using BareBones.Services.PlayerRegistry;

namespace BareBones.Scene.Lobby
{
    namespace BareBones.Scene
    {
        public enum PlayerParentTransform
        {
            None,
            GameObject,
            Path,
            Tag,
            Tag_Path
        }

        public class LocalPlayerLobby : MonoBehaviour, IMessageListener
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

            private IPlayerRegistry<Player> _registry;

            private IMessageBus _messageBus;
            private int _listenerHandle;
            private IObjectPoolCollection _pool;

            public void Start()
            {
                _registry = ResourceLocator._instance.Resolve<IPlayerRegistry<Player>>();
                _pool = ResourceLocator._instance.Resolve<IObjectPoolCollection>();
                _action = new InputAction();

                foreach (var path in joinActionPaths)
                {
                    _action.AddBinding(path);
                }

                _action.canceled += HandleRegisterPlayer;
                _action.Enable();
            }

            public void OnEnable()
            {
                if (_messageBus == null)
                {
                    _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
                }

                _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Player);
            }

            public void OnDisable()
            {
                if (_messageBus != null && _listenerHandle != -1)
                {
                    _messageBus.Unsubscribe(_listenerHandle);
                }
                _action.Dispose();
            }

            public void HandleRegisterPlayer(InputAction.CallbackContext context)
            {
                if (!EventSystem.current.IsPointerOverGameObject()
                    && _registry.AvailableSlots > 0
                    && !HasPlayerRegistered(context.action.activeControl.device))
                {
                    var newPlayerRoot = RegisterPlayer(context.control.device);

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

                    _messageBus.Send(MessageTopics.Player, MessageIds.PlayerJoined, gameObject, newPlayerRoot.Id);
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
                else if (taggedObjects == null || taggedObjects.Length == 0)
                {
                    Debug.LogWarning("No objects with tag " + tag + " exist.");
                }
                else
                {
                    Debug.LogWarning("More than one object with tag " + tag + " exists.");
                }

                return null;
            }

            public void HandleMessage(Message message)
            {
                if (message.id == MessageIds.PlayerCanceled)
                {
                    var registryIdx = (int)message.payload;
                    var playerRoot = _registry[registryIdx];

                    playerRoot._input.user.UnpairDevices();
                    _registry.DeregisterPlayer(registryIdx);

                    // xxx obj pool collection will clean this up
                    playerRoot.gameObject.SetActive(false);
                }
            }

            private Player RegisterPlayer(InputDevice device)
            {
                var (devices, controlScheme, deviceIds) = CreateInputConfiguration(device);

                var poolObjectHandle = _pool.Obtain((int)PoolIdEnum.Players);
                var poolObject = poolObjectHandle.Value.gameObject;
                var root = poolObject.GetComponent<Player>();
                var input = poolObject.GetComponent<PlayerInput>();
                var id = _registry.RegisterPlayer(root);

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
                return _registry.HasPlayerRegistered(root => root._deviceIds.Any(device.deviceId));
            }
        }
    }
}