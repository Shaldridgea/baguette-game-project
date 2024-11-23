using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to be inherited from to automatically cache instances of this object.
/// Optimisation to avoid GetComponent on frequently accessed objects
/// </summary>
/// <typeparam name="T"></typeparam>
public class CacheBehaviour<T> : MonoBehaviour
{
    private static Dictionary<int, T> cachedInstancesDict = new Dictionary<int, T>();

    private int thisID;

    private void Awake()
    {
        thisID = gameObject.GetInstanceID();
        if (!cachedInstancesDict.ContainsKey(thisID))
        {
            T thisComponent = GetComponent<T>();
            cachedInstancesDict.Add(thisID, thisComponent);
        }
        else
            cachedInstancesDict[thisID] = GetComponent<T>();
    }

    private void OnDestroy()
    {
        cachedInstancesDict.Remove(thisID);
    }

    /// <summary>
    /// Get cached component associated with this GameObject
    /// </summary>
    public static T GetCached(GameObject cacheObject)
    {
        if (cachedInstancesDict.TryGetValue(cacheObject.GetInstanceID(), out T data))
            return data;
        else
            return default;
    }
}