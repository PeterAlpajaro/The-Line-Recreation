using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class just attaches an ID to each object so that it can be identfied based on its prefab type
public class PooledObject : MonoBehaviour
{
    public int prefabID = -1;
    public Action<GameObject> onCollision;

    public bool isBeingReturned = false;

    // Size parameters.
    public float height;
    public float width;

    public bool TryReturnToPool()
    {
        if (isBeingReturned) return false;
        isBeingReturned = true;
        return true;
    }
}
