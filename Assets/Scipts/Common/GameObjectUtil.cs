using UnityEngine;

namespace BareBones.Common
{
    public static class GameObjectUtil
    {
        public static void ActivateHierarchyTree(this GameObject obj, bool value)
        {
            obj.SetActive(value);

            for (var j = 0; j < obj.transform.childCount; j++)
            {
                obj.transform.GetChild(j).gameObject.SetActive(value);
            }
        }

        public static bool HasParent(this GameObject obj, GameObject parent)
        {
            var current = obj;

            while (current != null && current != parent)
            {
                current = current.transform.parent != null
                            ? current.transform.parent.gameObject
                            : null;
            }

            return current == parent;
        }
    }
}
