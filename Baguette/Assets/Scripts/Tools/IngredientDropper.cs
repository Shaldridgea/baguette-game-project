using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic for draggable item that spawns several ingredients together to be dropped
/// </summary>
public class IngredientDropper : MonoBehaviour
{
    [SerializeField]
    private IngredientPart partPrefab;

    [SerializeField]
    private int dropAmount;

    [SerializeField]
    private float minSpawnDist = 0.3f;

    [SerializeField]
    private float maxSpawnDist = 0.8f;

    private TouchableDrag touchDrag;

    private IngredientPart[] parts;

    void Start()
    {
        touchDrag = GetComponent<TouchableDrag>();
        touchDrag.DropEvent += CheckForDrop;
        parts = new IngredientPart[dropAmount];

        float spawnAngle = 0f;
        float angleDelta = 360f / dropAmount;
        // Spawn all our ingredients in a random circle
        for (int i = 0; i < dropAmount; ++i)
        {
            parts[i] = Instantiate(partPrefab, transform);
            float r = spawnAngle * Mathf.Deg2Rad;
            parts[i].transform.localPosition = new Vector3(Mathf.Cos(r), Mathf.Sin(r)) * Random.Range(minSpawnDist, maxSpawnDist);
            parts[i].transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 359f));
            spawnAngle += angleDelta;
        }
        FlowManager.Instance.StateEnd += CheckForBakeEnd;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateEnd -= CheckForBakeEnd;
    }

    private void CheckForBakeEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Baking)
            return;

        Destroy(gameObject);
    }

    private void CheckForDrop(PlayerHand hand)
    {
        LeanTween.cancel(gameObject);
        // Set the ingredients we're holding to start moving
        for (int i = 0; i < dropAmount; ++i)
            parts[i].Init();
        
        Destroy(touchDrag);
    }

    private void OnTransformChildrenChanged()
    {
        // Our ingredients will either change
        // parent or destroy themselves so
        // destroy us if they've all left
        if (transform.childCount == 0)
            Destroy(gameObject);
    }
}