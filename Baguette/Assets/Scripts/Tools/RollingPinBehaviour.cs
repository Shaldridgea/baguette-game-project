using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic controlling rolling pin interaction with dough
/// </summary>
public class RollingPinBehaviour : MonoBehaviour
{
    [SerializeField]
    private float rollingIncreaseAmount;

    [SerializeField]
    private float minRollingSpeed = 0.2f;

    [SerializeField]
    private Vector2 startBaguetteSize = new Vector2(0.6f, 0.6f);

    [SerializeField]
    private Vector2 normalBaguetteSize = new Vector2(1.6f, 0.3f);

    private TouchableDrag pinDrag;

    private PlayerHand holdingHand;

    // Start is called before the first frame update
    void Start()
    {
        pinDrag = GetComponentInParent<TouchableDrag>();
        pinDrag.HoldEvent += HandToggle;
        pinDrag.DropEvent += HandToggle;
    }

    private void HandToggle(PlayerHand hand)
    {
        if (holdingHand == null)
            holdingHand = hand;
        else
            holdingHand = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Dough"))
            return;
        
        if (holdingHand)
        {
            // Must be moving with enough force to roll dough
            if (Mathf.Abs(holdingHand.HandMovement.y) < minRollingSpeed)
                return;

            BreadData data = BreadData.GetCached(collision.gameObject);
            if (!data)
                return;

            // Can't roll cooked bread
            if (data.Finished)
                return;

            data.IncreaseRolling(rollingIncreaseAmount);
            LeanTween.cancel(data.DoughSpriteTransform.gameObject);
            // LerpUnclamped allows for dough to be over-rolled
            LeanTween.scale(data.DoughSpriteTransform.gameObject,
                Vector3.LerpUnclamped(startBaguetteSize, normalBaguetteSize, data.RollingStage / BreadData.IDEAL_ROLLING_STAGE), rollingIncreaseAmount)
                .setEaseOutCubic()
                .setOnComplete(data.UpdateParentCollider);
        }
    }
}