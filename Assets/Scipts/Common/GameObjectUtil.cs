using UnityEngine;

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
}

