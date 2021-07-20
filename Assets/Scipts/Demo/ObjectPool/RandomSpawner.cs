using BareBones.Common;
using BareBones.Services.ObjectPool;

using UnityEngine;

public class RandomSpawner : MonoBehaviour
{
    public float _areaRadius = 10.0f;
    public float _minScale = 0.1f;
    public float _maxScale = 2.0f;

    public float _spawnInterval = 1.0f;

    public int _minSpawns = 0;
    public int _maxSpawns = 5;

    public OnEndOfLifeAction _onEndOfLifeAction = OnEndOfLifeAction.Disable;

    private IObjectPoolCollection _objectPool;
    private float _lastSpawnTime = -99999.0f;

    public void Start()
    {
        _objectPool = ResourceLocator._instance.Resolve<IObjectPoolCollection>();

        Debug.Assert(_objectPool != null);
    }


    public void Update()
    {
        if (Time.time - _lastSpawnTime > _spawnInterval)
        {
            var count = Random.Range(_minSpawns, _maxSpawns);

            for (var i = 0; i < count; i++)
            {
                TrySpawnObject();
            }

            _lastSpawnTime = Time.time;
        }
    }

    private void TrySpawnObject()
    { 
        var poolIdx = Random.Range(0, _objectPool.PoolCount);
            
        if (_objectPool[poolIdx].Available > 0)
        {
            var objReference = _objectPool.Obtain(poolIdx);

            if (objReference.gameObject != null)
            {
                var gameObj = objReference.gameObject;
                var lifeTimeBehaviour = gameObj.GetComponent<LifeTimeTrackingBehaviour>();

                gameObj.transform.localScale = Vector3.one * Random.Range(_minScale, _maxScale);
                gameObj.transform.position = transform.position + Random.insideUnitSphere * _areaRadius;
                gameObj.transform.parent = transform;

                lifeTimeBehaviour._handle = objReference;
                lifeTimeBehaviour._actionAtEndOfLife = _onEndOfLifeAction; 
            }
        }
    }
}
