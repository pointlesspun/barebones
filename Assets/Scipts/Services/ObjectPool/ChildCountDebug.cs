using UnityEngine;

namespace BareBones.Services.ObjectPool
{

    public class ChildCountDebug : MonoBehaviour
    {
        private string _baseName;

        public void Start()
        {
            _baseName = gameObject.name;
        }

        public void Update()
        {
            gameObject.name = _baseName + " (" + transform.childCount + " children)";
        }
    }
}
