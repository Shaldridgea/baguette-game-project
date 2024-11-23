using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic and data for a single ingredient instance that can be put on dough
/// </summary>
public class IngredientPart : MonoBehaviour
{
    [SerializeField]
    private Consts.BreadType ingredientType;

    [SerializeField]
    private float minAcceleration;

    [SerializeField]
    private float maxAcceleration;

    [SerializeField]
    private float intoBreadMinDist = 0.1f;

    [SerializeField]
    private float intoBreadMaxDist = 1.5f;

    [SerializeField]
    private Sprite[] spriteVariants;

    private Vector3 partVelocity = new Vector3(0f, -1f);

    private Vector3 partAcceleration;

    // yGoal by default is below the screen to disappear
    private float yGoal = -10f;

    private Transform breadGoal = null;

    private SpriteRenderer spriteRend;

    private void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        if (spriteVariants != null && spriteVariants.Length > 0)
            spriteRend.sprite = spriteVariants[Random.Range(0, spriteVariants.Length)];
        enabled = false;
    }

    /// <summary>
    /// Initialise the ingredient's physics
    /// </summary>
    public void Init()
    {
        enabled = true;
        partAcceleration = new Vector3(0f, Random.Range(minAcceleration, maxAcceleration));
        // Attach immediately to a piece of dough if it's under us when we start
        Collider2D dough = CollisionHandler.CheckPositionForCollider(transform.position, Consts.TOUCHABLE_MASK, "Dough");
        if (dough != null)
        {
            // If ingredient is already directly on bread we don't want it to move very far
            intoBreadMaxDist = intoBreadMinDist * 2f;
            TrySetDoughTarget(dough.gameObject);
        }
    }

    void FixedUpdate()
    {
        if (transform.localPosition.y <= yGoal)
        {
            if (breadGoal != null)
            {
                // Attach ourselves to this bread and stop
                // moving by removing this component
                transform.SetParent(breadGoal, true);
                spriteRend.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                Destroy(this);
            }
            else
                Destroy(gameObject);
            return;
        }
        transform.localPosition += partVelocity * Time.deltaTime;
        partVelocity += partAcceleration * Time.deltaTime;
    }

    private void TrySetDoughTarget(GameObject newDough)
    {
        if (!enabled)
            return;

        if (breadGoal != null)
            return;

        if (!newDough.CompareTag("Dough"))
            return;

        BreadData data = BreadData.GetCached(newDough);

        if (!data)
            return;

        // Don't attach ourselves to dough that isn't ready yet
        if (data.RollingStage < BreadData.IDEAL_ROLLING_STAGE)
            return;

        // When we enter a valid piece of dough, mark how far
        // to keep moving before stopping on the dough
        yGoal = transform.localPosition.y - Random.Range(intoBreadMinDist, intoBreadMaxDist);
        breadGoal = newDough.transform;
        data.IncreaseIngredientCount(ingredientType);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Attach ourselves to dough if we fall onto it
        TrySetDoughTarget(collision.gameObject);
    }
}