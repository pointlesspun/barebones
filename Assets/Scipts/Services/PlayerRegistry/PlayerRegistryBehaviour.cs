using UnityEngine;
using UnityEngine.InputSystem;

using BareBones.Common;
using BareBones.Services.Messages;
using BareBones.Services.ObjectPool;

namespace BareBones.Services.PlayerRegistry
{
    public class PlayerRegistryBehaviour : MonoBehaviour, IMessageListener
    {
        public int maxPlayers = 3;

        public int initialActivePlayers = 0;

        private PlayerRegistry<Player> _registry;
        private IObjectPoolCollection<GameObject> _poolCollection;
        private int _poolIdx;
        private ILocationProvider _startLocationProvider;
        private IMessageBus _messageBus;
        private int _listenerHandle;

        public void Awake()
        {
            if (_registry == null && !ResourceLocator._instance.Contains<IPlayerRegistry<Player>>())
            {
                _registry = ResourceLocator._instance.Register<IPlayerRegistry<Player>, PlayerRegistry<Player>>(maxPlayers);
            }
        }

        public void OnEnable()
        {
            if (_messageBus == null)
            {
                _messageBus = ResourceLocator._instance.Resolve<IMessageBus>();
            }

            _listenerHandle = _messageBus.Subscribe(this, MessageTopics.Scene);
        }

        public void OnDisable()
        {
            if (_messageBus != null && _listenerHandle != -1)
            {
                _messageBus.Unsubscribe(_listenerHandle);
            }
        }


        public void Start()
        {
            _poolCollection = ResourceLocator._instance.Resolve<IObjectPoolCollection<GameObject>>();
            Debug.Assert(_poolCollection != null, "Expected to find a IObjectPoolCollection<GameObject> declared in the ResourceLocator.");
            _poolIdx = _poolCollection.FindPoolIdx(PoolIdEnum.Players);
            Debug.Assert(_poolIdx != -1, "No pool with pool id " + PoolIdEnum.Players + " declared in the object poolCollection.");

            // This handles the case where the starting scene is not the lobby but
            // some in-game scene and we want to run/test the scene. Normally
            // the lobby would do the setup for the players, when starting without 
            // the lobby the registry can create those players itself.
            for (var i = 0; i < initialActivePlayers; i++)
            {
                var playerHandle = _poolCollection.Obtain((int)PoolIdEnum.Players);

                if (playerHandle.HasReference)
                {
                    var playerObject = _poolCollection.Dereference(playerHandle);
                    var rootId = _registry.RegisterPlayer(playerObject.GetComponent<Player>());
                    var root = _registry[rootId];

                    root._deviceIds = CaptureDeviceIds(i);
                    playerObject.ActivateHierarchyTree(true);
                }
                else
                {
                    Debug.LogError("No more player objects available in the pool");
                }
            }
        }

        public void OnDestroy()
        {
            if (_registry != null)
            {
                ResourceLocator._instance.Deregister<IPlayerRegistry<Player>>();
                _registry = null;
            }
        }

        public void HandleMessage(Message message)
        {
            if (message.id == MessageIds.SceneStarted)
            {
                // scene has begun, activate the players put them in their starting location
                foreach (var root in _registry)
                {
                    root.gameObject.ActivateHierarchyTree(true);
                }

                _startLocationProvider = GetComponent<ILocationProvider>();

                if (_startLocationProvider != null)
                {
                    _startLocationProvider.AssignLocations(_registry, _registry.PlayerCount);
                }
                else
                {
                    Debug.LogWarning("No location provider in player registry, players will be put at their default position.");
                }
            }
        }

        /**
         * Try to match best guess devices to playerIndex where
         * playerIndex 0 will be matched to keyboard/mouse and the rest to gamepads
         */
        private int[] CaptureDeviceIds(int playerIndex)
        {
            var devices = InputSystem.devices;

            if (playerIndex == 0)
            {
                var mouse = devices.FirstOrDefault(d => d.name.IndexOf("Mouse") >= 0);
                var keyboard = devices.FirstOrDefault(d => d.name.IndexOf("Keyboard") >= 0);

                return (mouse != null && keyboard != null)
                    ? new int[] { mouse.deviceId, keyboard.deviceId }
                    : new int[0];
            }

            var skip = playerIndex - 1;

            for (var i = 0; i < devices.Count; i++)
            {
                if (devices[i].enabled && devices[i] is Gamepad)
                {
                    if (skip == 0)
                    {
                        return new int[] { devices[i].deviceId };
                    }

                    skip--;
                }
            }

            return new int[0];
        }
    }
}