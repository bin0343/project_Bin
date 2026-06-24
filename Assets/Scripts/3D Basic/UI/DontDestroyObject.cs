using UnityEngine;
using System.Collections.Generic;

public class DontDestroyObject : MonoBehaviour
{
    private static HashSet<int> processedObjects = new HashSet<int>();

    private void Awake()
    {
        int instanceID = gameObject.GetInstanceID();
        if (processedObjects.Contains(instanceID))
        {
            return;
        }

        var objs = FindObjectsOfType<DontDestroyObject>();
        foreach (var obj in objs)
        {
            if (obj != this && obj.gameObject.name == gameObject.name)
            {
                Destroy(gameObject);
                return;
            }
        }

        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        processedObjects.Add(instanceID);
    }
}