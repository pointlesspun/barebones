using System;
using System.Collections.Generic;
using UnityEngine;

public enum TargetGroupEnum
{
    None,
    Players,
    TargetProperty
}

public class Spawner : MonoBehaviour
{
    public PoolIdEnum poolId;
    
    public TargetGroupEnum targetSelection = TargetGroupEnum.Players;
    public GameObject target;

    public Component locationProviderComponent;

    public int max = 3;

    public int maxAlive = -1;

    public int maxPerIteration = 1;

    public float interval = 1.0f;

    public float initialDelay = 0.0f;

    private int _current = 0;
    private List<GameObjectMeta> _aliveObjects = new List<GameObjectMeta>();
    private float _lastSpawnTime = -1.0f;
    private GameObject[] _activePlayers;

    // Remember what player was selected as a target last time. We don't
    // want to pick on the same player every time.
    private int _lastTargetedPlayer = -1;

    void Update()
    {
        // have to put a small delay in to allow players to spawn
        if (Time.timeSinceLevelLoad > initialDelay
            && (maxAlive < 0 || _aliveObjects.Count < maxAlive)
            && Time.time - _lastSpawnTime > interval)
        {
            for (var i = 0; i < maxPerIteration; i++)
            {
                SpawnObject();
            }
        }

        UpdateAliveCount();
    }

    private void SpawnObject()
    {
        var spawnedObject = ObjectPoolCollection.instance.Obtain((int)poolId);

        if (spawnedObject != null)
        {
            // spawner will release the object to the pool
            spawnedObject.deferRelease = true;

            if (locationProviderComponent != null)
            {
                if (locationProviderComponent is ILocationProvider provider)
                {
                    spawnedObject.transform.localPosition = provider.GetNextLocation();
                }
                else
                {
                    Debug.LogError("Location provider component does not implement " + nameof(ILocationProvider));
                }
            }
            else
            {
                spawnedObject.transform.localPosition = gameObject.transform.position;
            }

            SelectTargetFor(spawnedObject.gameObject, targetSelection, target);

            _lastSpawnTime = Time.time;

            _current++;
            _aliveObjects.Add(spawnedObject);

            if (max >= 0 && _current >= max)
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("Spawner cannot get any more objects from pool " + poolId + ".");
        }
    }

    private void UpdateAliveCount()
    {
        var i = 0;
        while( i < _aliveObjects.Count)
        {
            var meta = _aliveObjects[i];
            if (meta.isReleased)
            {
                _aliveObjects.RemoveAt(i);
                ObjectPoolCollection.instance.Release(meta);
            }
            else
            {
                i++;
            }
        }
    }

    private void SelectTargetFor(GameObject obj, TargetGroupEnum group, GameObject target)
    {
        switch (group)
        {
            case TargetGroupEnum.None:
                break;

            case TargetGroupEnum.Players:

                if (_activePlayers == null || _activePlayers.Length == 0)
                {
                    _activePlayers = GameObject.FindGameObjectsWithTag("Player");
                }

                var playerIndex = SelectPlayer(_activePlayers, _lastTargetedPlayer);
                
                // check if not all players are dead
                if (playerIndex >= 0 && playerIndex < _activePlayers.Length )
                {
                    AssignTarget(obj, _activePlayers[playerIndex]);
                    _lastTargetedPlayer = playerIndex;
                }
                else
                {
                    Debug.LogWarning("Spawner.SelectTargetFor: playerIndex " + playerIndex + " not valid.");
                }
                break;
            case TargetGroupEnum.TargetProperty:
                AssignTarget(obj, target);
                break;
            default:
                throw new NotImplementedException("no case for: " + group);
        }
    }

    private void AssignTarget(GameObject obj, GameObject target)
    {
        var components = obj.GetComponents<ITarget>();

        if (components != null && components.Length > 0)
        {
            for (var  i = 0; i < components.Length; i++)
            {
                components[i].SetTarget(target);
            }
        }
    }

    private int SelectPlayer(GameObject[] players, int lastSelection)
    {
        if (lastSelection < 0)
        {
            return UnityEngine.Random.Range(0, players.Length);
        }

        var playerCount = players.Length;

        for (var i = 0; i < playerCount; i++)
        {
            var testIndex = (i + lastSelection + 1) % playerCount;
            if (players[testIndex] != null)
            {
                return testIndex;
            }
        }

        return lastSelection;
    }
}

