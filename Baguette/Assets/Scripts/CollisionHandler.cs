using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility functions for 2D collision checking and retrieval
/// </summary>
public static class CollisionHandler
{
    private const int MAX_RESULTS = 5;

    private static Collider2D[] colliderArray = new Collider2D[MAX_RESULTS];

    /// <summary>
    /// Find all colliders touching the passed collider on the passed layers
    /// </summary>
    public static Collider2D[] GetCurrentCollisions(Collider2D checkCollider, int bitmaskLayer)
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        filter.SetLayerMask(bitmaskLayer);
        int resultCount = checkCollider.OverlapCollider(filter, colliderArray);

        if (resultCount == 0)
            return null;

        if (resultCount < MAX_RESULTS)
        {
            Collider2D[] actualResults = new Collider2D[resultCount];
            Array.Copy(colliderArray, actualResults, resultCount);
            return actualResults;
        }

        return colliderArray;
    }

    /// <summary>
    /// Find the first collider that overlaps the passed point on the passed layers, and matching the tags
    /// </summary>
    /// <param name="checkPos"></param>
    /// <param name="bitmaskLayer"></param>
    /// <param name="tags"></param>
    /// <returns></returns>
    public static Collider2D CheckPositionForCollider(Vector3 checkPos, int bitmaskLayer, params string[] tags)
    {
        int resultCount = Physics2D.OverlapPointNonAlloc(checkPos, colliderArray, bitmaskLayer);

        if (resultCount == 0)
            return null;

        if (tags.Length == 0)
            return colliderArray[0];

        for (int i = 0; i < resultCount; ++i)
        {
            for (int j = 0; j < tags.Length; ++j)
            {
                if (!colliderArray[i].CompareTag(tags[j]))
                    continue;

                return colliderArray[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Attach the first object to the second object
    /// </summary>
    public static void AttachToObject(Transform attachTrans, Transform parentTrans)
    {
        if (attachTrans == null || parentTrans == null)
            return;

        attachTrans.SetParent(parentTrans, true);
        Vector3 localPos = attachTrans.localPosition;
        localPos.z = 0f;
        attachTrans.localPosition = localPos;
    }
}