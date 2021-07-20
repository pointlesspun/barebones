using BareBones.Common;
using UnityEngine;

public class RotateBehaviour : MonoBehaviour
{
    public InitialValue _initialRotationValue = InitialValue.Default;
    public InitialValue _initialSpeedValue = InitialValue.Default;

    public float _randomSpeedMin = 0;
    public float _randomSpeedMax = 720.0f;

    public Vector3 _defaultAngularRotation = Vector3.zero;
    public float _defaultSpeed = 180.0f;

    private Vector3 _currentAngularRotation;
    private float _speed;


    public void OnEnable()
    {
        switch (_initialRotationValue)
        {
            case InitialValue.Zero:
                _currentAngularRotation = Vector3.zero;
                break;
            case InitialValue.Random:
                _currentAngularRotation = Random.onUnitSphere;
                break;
            case InitialValue.Default:
            default:
                _currentAngularRotation = _defaultAngularRotation;
                break;
        }

        switch (_initialSpeedValue)
        {
            case InitialValue.Zero:
                _speed = 0;
                break;
            case InitialValue.Random:
                _speed = Random.Range(_randomSpeedMin, _randomSpeedMax);
                break;
            case InitialValue.Default:
            default:
                _speed = _defaultSpeed;
                break;
        }
    }

    public void Update()
    {
        transform.rotation *= Quaternion.Euler((Time.deltaTime * _speed * _currentAngularRotation));
    }
}

