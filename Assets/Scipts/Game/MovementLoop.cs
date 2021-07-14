using UnityEngine;

namespace BareBones.Game
{
    public class MovementLoop : MonoBehaviour
    {
        public AnimationCurve xSpeed;
        public float speedScale = 1.0f;

        public float timeScale = 1.0f;

        private Vector3 _speed = Vector3.zero;

        public void Update()
        {
            float time = (Time.time % timeScale) / timeScale;

            _speed = new Vector3(xSpeed.Evaluate(time), 0, 0);
            transform.position += _speed * Time.deltaTime * speedScale;
        }
    }
}