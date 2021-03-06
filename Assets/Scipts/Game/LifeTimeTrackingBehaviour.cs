using BareBones.Common;
using BareBones.Services.ObjectPool;
using UnityEngine;

public enum OnEndOfLifeAction
{
    None,
    Disable,
    Destroy,
    Release
}

public class LifeTimeTrackingBehaviour : MonoBehaviour
{
    public OnEndOfLifeAction _actionAtEndOfLife = OnEndOfLifeAction.None;

    public float _minLifeTime = 1.0f;
    public float _maxLifeTime = 10.0f;

    public PoolObjectHandle _handle;

    private float _lifeTime = -1;
    private float _startTime = -1;

    public bool HasExceededLifeTime => _startTime >= 0 && (Time.time - _startTime) > _lifeTime;

    private IObjectPoolCollection<GameObject> _pool;

    public void OnEnable()
    {
        if (_startTime < 0)
        {
            _lifeTime = Random.Range(_minLifeTime, _maxLifeTime);
            _startTime = Time.time;
        }

        if (_pool == null)
        {
            _pool = ResourceLocator._instance.Resolve<IObjectPoolCollection<GameObject>>();
        }
    }

    public void Update()
    {
        if (HasExceededLifeTime)
        {
            switch (_actionAtEndOfLife)
            {
                case OnEndOfLifeAction.None:
                    break;
                case OnEndOfLifeAction.Disable:
                    gameObject.SetActive(false);
                    _startTime = -1.0f;
                    break;
                case OnEndOfLifeAction.Destroy:
                    GameObject.Destroy(gameObject);
                    break;
                case OnEndOfLifeAction.Release:
                    _pool.Release(_handle);
                    _startTime = -1.0f;
                    break;
            }
        }
    }
}

