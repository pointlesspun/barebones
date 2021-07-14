using UnityEngine;

using BareBones.Common.Behaviours;

namespace BareBones.Game
{
    public class FollowTarget : MonoBehaviour, ITarget
    {
        public GameObject target;

        private Rigidbody _body;

        public float maxVelocity = 1.0f;

        public Vector3 velocityDampening = Vector3.one;

        public float inputThreshold = 0.5f;

        public float speed;
        public float maxRotationSpeed = 180;

        private Vector3 currentDirection;

        private bool hasBeenInitialized = false;

        void Start()
        {
            _body = gameObject.GetComponent<Rigidbody>();

            Debug.Assert(_body != null);
        }

        void OnEnable()
        {
            hasBeenInitialized = false;
        }

        void Update()
        {
            if (target != null)
            {

                if (!hasBeenInitialized)
                {
                    // custom init step since we're pooling objects
                    currentDirection = (target.transform.position - gameObject.transform.position).normalized;

                    transform.rotation = Quaternion.identity;

                    _body.velocity = Vector3.zero;
                    _body.angularVelocity = Vector3.zero;


                    hasBeenInitialized = true;
                }

                Vector3 targetDirection = (target.transform.position - gameObject.transform.position).normalized;

                currentDirection = Vector3.RotateTowards(
                    currentDirection,
                    targetDirection,
                    maxRotationSpeed * Time.deltaTime,
                    0
                );

                _body.AddForce(currentDirection * speed);
                _body.velocity = PhysicsControl.ClampVelocity(_body.velocity, maxVelocity);
            }
        }

        public void SetTarget(GameObject target)
        {
            this.target = target;
        }

        private void OnDrawGizmos()
        {
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(gameObject.transform.position, target.transform.position);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(gameObject.transform.position, gameObject.transform.position + currentDirection * 10.0f);
            }
        }
    }
}