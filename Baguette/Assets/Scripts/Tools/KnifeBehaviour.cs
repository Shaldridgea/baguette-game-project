using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic for knife interaction with dough
/// </summary>
public class KnifeBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform slashPrefab;

    [SerializeField]
    private Transform bladeColliderTransform;

    [SerializeField]
    private int maxSlashes;

    [SerializeField]
    private int slashesPerCollision;

    [SerializeField]
    private float speedThreshold = 0.3f;

    private TouchableDrag knifeDrag;

    private PlayerHand holdingHand;

    // Start is called before the first frame update
    void Start()
    {
        knifeDrag = GetComponentInParent<TouchableDrag>();
        knifeDrag.HoldEvent += HandToggle;
        knifeDrag.DropEvent += HandToggle;
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

        if (!holdingHand)
            return;

        // Knife needs to be moving fast enough to cut the dough
        if (holdingHand.HandMovement.y < speedThreshold)
            return;
        
        BreadData data = BreadData.GetCached(collision.gameObject);
        if (!data)
            return;

        // Can't cut bread that's already cooked
        if (data.Finished)
            return;

        // Can't cut bread that isn't rolled enough
        if (data.RollingStage < BreadData.IDEAL_ROLLING_STAGE)
            return;
        
        // Need to offset the slashes made if this knife makes multiple at once
        float offsetIncrease = 0.75f;
        float slashOffset = -(offsetIncrease * slashesPerCollision) / 2f;
        for (int i = 0; i < slashesPerCollision; ++i)
        {
            if (data.SlashCount >= maxSlashes)
                return;

            // Create and attach slash to dough
            float slashX = bladeColliderTransform.position.x + slashOffset;
            Transform slashTrans = Instantiate(slashPrefab, new Vector3(slashX, collision.transform.position.y), slashPrefab.rotation);
            CollisionHandler.AttachToObject(slashTrans, data.DoughSpriteTransform);

            // Animate slash scaling to normal size to look like it's been cut
            Vector3 startScale = slashTrans.localScale;
            startScale.y *= 0.25f;
            LeanTween.scaleY(slashTrans.gameObject, slashTrans.localScale.y, 0.13f);
            slashTrans.localScale = startScale;
            data.KnifeSlash();
            slashOffset += offsetIncrease;
        }
    }
}