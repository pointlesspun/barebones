
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

using BareBones.Common;

namespace BareBones.Game
{
    /**
     * Translates input into movement
     */
    public class AgentController : MonoBehaviour, IAgentController
    {
        public float _speed;

        public Bounds _boundingBox = new Bounds(Vector3.zero, new Vector3(2, 2, 2));

        public float _maxVelocity = 1.0f;

        public float _velocityDampening = 0.4f;

        public float _inputThreshold = 0.1f;

        private Rigidbody _body;

        private readonly List<IWeapon> _weapons = new List<IWeapon>();

        private bool _isFiring = false;
        private Vector2 _directionInput = Vector2.zero;

        public bool IsActive => gameObject.activeSelf;

        public void Start()
        {
            _body = gameObject.GetComponent<Rigidbody>();

            Debug.Assert(_body != null);

            for (var i = 0; i < transform.childCount; i++)
            {
                var childObj = transform.GetChild(i).gameObject;
                if (childObj.CompareTag("Weapon"))
                {
                    _weapons.Add(childObj.GetComponent<IWeapon>());
                }
            }
        }

        public void Update()
        {
            if (_isFiring)
            {
                FireWeapons();
            }

            float dampening = _directionInput.magnitude > _inputThreshold ? 1.0f : _velocityDampening;

            _body.AddForce(_directionInput * _speed);
            _body.velocity = PhysicsControl.ClampVelocity(_body.velocity, dampening, _maxVelocity);

            gameObject.transform.position = PhysicsControl.ClampPosition(gameObject.transform.position, _boundingBox);
        }

        public void ChangeFiringState(InputAction.CallbackContext context)
        {
            _isFiring = !context.canceled;
        }

        public void UpdateDirection(InputAction.CallbackContext context)
        {
            _directionInput = context.ReadValue<Vector2>();
        }

        public void FireWeapons()
        {
            for (var i = 0; i < _weapons.Count; i++)
            {
                _weapons[i].Fire(((MonoBehaviour)_weapons[i]).gameObject.transform.localPosition);
            }
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(_boundingBox.center, _boundingBox.extents * 2.0f);
        }

        public void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }
    }
}