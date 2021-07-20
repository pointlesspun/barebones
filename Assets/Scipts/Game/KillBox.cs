using UnityEngine;

namespace BareBones.Game
{
    public class KillBox : MonoBehaviour
    {
        public Bounds killBoxDimensions;

        void Update()
        {
            if (!killBoxDimensions.Contains(gameObject.transform.position))
            {
                // xxx assumption here is this object is pooled and will be cleaned up 
                // the objectpool's sweep
                gameObject.SetActive(false);
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(killBoxDimensions.center, killBoxDimensions.extents * 2.0f);
        }
    }
}