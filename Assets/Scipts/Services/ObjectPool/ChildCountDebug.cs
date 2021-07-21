using UnityEngine;

namespace BareBones.Services.ObjectPool
{
    /**
     * Debug behaviour changing the game object's name by adding
     * the number of children added to its transform eg
     * if the name is "foo" the name will be replaced during updated 
     * to "foo (3 children)" if it has three children. 
     */
    public class ChildCountDebug : MonoBehaviour
    {
        /** Name of the gameobject when Start is called */
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
