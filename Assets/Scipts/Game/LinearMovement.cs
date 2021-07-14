using UnityEngine;

namespace BareBones.Game
{
    public class LinearMovement : MonoBehaviour
    {
        public float force = 1;
        public float initialVelocity = 1;

        private Rigidbody _body;

        void Start()
        {
            _body = gameObject.GetComponent<Rigidbody>();

            Debug.Assert(_body != null);

            _body.velocity = gameObject.transform.up * initialVelocity;
        }

        private void Update()
        {
            _body.AddForce(gameObject.transform.up * force);
        }
    }

}