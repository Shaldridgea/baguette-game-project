using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Individual player hand behaviour and visuals control
/// </summary>
public class PlayerHand : MonoBehaviour
{
    // VISUALS
    [SerializeField]
    private Transform handVisuals;

    [SerializeField]
    private Transform sleeve;

    [SerializeField]
    private LineRenderer handLine;

    [SerializeField]
    [Range(-1f, 1f)]
    private float armAngleSteepness;

    [SerializeField]
    private SpriteRenderer[] spriteRenderers;

    [SerializeField]
    [Range(0, 0.1f)]
    private float boilAnimationSpeed;

    public bool FlipSprites;

    private SpriteRenderer[] boilRenderers;

    private MaterialPropertyBlock boilPropBlock;

    private int boilSnapID;

    private const int LINE_TOP_POINT = 0;

    private const int LINE_BOTTOM_POINT = 1;

    // GAMEPLAY
    private Collider2D handCollider;

    private Vector3 startPosition;

    public bool InUse { get; set; }

    public bool IsCarrying => CarryingTouchable != null;

    public TouchableDrag CarryingTouchable { get; private set; }

    public Vector3 HandMovement { get; private set; }

    private Vector3 lastFramePos;

    private class TouchableOrderComparer : IComparer<Collider2D>
    {
        public int Compare(Collider2D t1, Collider2D t2)
        {
            Touchable touchT1 = Touchable.GetCached(t1.gameObject);
            Touchable touchT2 = Touchable.GetCached(t2.gameObject);
            if (touchT1 == null)
            {
                if (touchT2 == null)
                    return 0;
                else
                    return 1;
            }
            else
            {
                // Compare which touchables are higher up in sorting order/closer to screen
                if (touchT2 == null)
                    return -1;
                else if (touchT1.TouchableSortOrder < touchT2.TouchableSortOrder)
                    return 1;
                else if (touchT1.TouchableSortOrder > touchT2.TouchableSortOrder)
                    return -1;
                else
                    return 0;
            }
        }
    }

    void Start()
    {
        handCollider = GetComponent<Collider2D>();
        startPosition = transform.position;
        lastFramePos = transform.position;

        List<SpriteRenderer> boilers = new List<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; ++i)
        {
            if (FlipSprites)
                spriteRenderers[i].flipX = true;

            if (spriteRenderers[i].sharedMaterial.name.Contains("Boil"))
                boilers.Add(spriteRenderers[i]);
        }
        boilRenderers = boilers.ToArray();
        boilPropBlock = new MaterialPropertyBlock();
        boilSnapID = Shader.PropertyToID("_TimeSnap");
        SetBoil(false);
    }

    /// <summary>
    /// Move the hand by the passed vector in local space
    /// </summary>
    public void Move(Vector3 moveVec)
    {
        transform.localPosition += moveVec;
        UpdateHandVisuals(moveVec);
    }

    private void FixedUpdate()
    {
        HandMovement = transform.position - lastFramePos;
        lastFramePos = transform.position;
    }

    /// <summary>
    /// Attach a touchable object to this hand to be carried around
    /// </summary>
    public void AttachTouchable(Touchable carryable)
    {
        if (carryable is not TouchableDrag)
            return;

        CarryingTouchable = (TouchableDrag)carryable;
        carryable.transform.SetParent(CarryingTouchable.MatchHandRotation ? handVisuals.transform : transform, true);
        LeanTween.cancel(carryable.gameObject);
        CarryingTouchable.Hold(this);
    }

    /// <summary>
    /// Finds the touchable colliding with this hand that is visibly closest to the screen
    /// </summary>
    /// <returns>The found touchable, if any</returns>
    public Touchable FindTouchable()
    {
        Collider2D[] localCollisions = CollisionHandler.GetCurrentCollisions(handCollider, Consts.TOUCHABLE_MASK);
        if (localCollisions == null)
            return null;

        // Sort our touchables by sprite ordering, so we return the one
        // that is visibly above everything else we're touching
        if(localCollisions.Length > 1)
            Array.Sort(localCollisions, new TouchableOrderComparer());
        return Touchable.GetCached(localCollisions[0].gameObject);
    }

    public void InteractTouchable(Touchable touch)
    {
        if (touch == null)
            return;

        touch.InteractHand(this);
    }

    /// <summary>
    /// Drops the touchable object this hand is carrying, if any
    /// </summary>
    /// <returns>Whether something was dropped</returns>
    public bool TryDetachTouchable()
    {
        if (CarryingTouchable == null)
            return false;

        Transform newTouchParent;
        Collider2D collider = CollisionHandler.CheckPositionForCollider(transform.position, CarryingTouchable.DropMask, CarryingTouchable.DropTags);
        if (collider != null)
            newTouchParent = collider.transform;
        else
        {
            // If the touchable was not dropped on a valid surface,
            // set it to fall down on this screen
            newTouchParent = CameraController.CurrentScreenTransform;
            CarryingTouchable.Fall();
        }

        // Null our carrying object in case when we drop, something
        // checks for whether this hand is carrying something still
        TouchableDrag tempDrag = CarryingTouchable;
        CarryingTouchable = null;

        CollisionHandler.AttachToObject(tempDrag.transform, newTouchParent);
        tempDrag.Drop(this);

        return true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var cached = Touchable.GetCached(collision.gameObject);
        
        if(cached != null)
            cached.EnterHand(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        var cached = Touchable.GetCached(collision.gameObject);

        if (cached != null)
            cached.LeaveHand(this);
    }

    // VISUALS

    /// <summary>
    /// Update the position and rotation of the hand and arm visuals
    /// </summary>
    /// <param name="moveVec">How much the hand has moved</param>
    private void UpdateHandVisuals(Vector3 moveVec)
    {
        moveVec.y = 0f;
        // Setting the position of the arm's "shoulder" point
        handLine.SetPosition(LINE_BOTTOM_POINT, handLine.GetPosition(LINE_BOTTOM_POINT) - moveVec * armAngleSteepness);

        // How much to rotate the hand/arm based on where it started and how far it's moved
        Vector3 angleVec = transform.position - (startPosition + handLine.GetPosition(LINE_BOTTOM_POINT));
        float zRot = (Mathf.Atan2(angleVec.y, angleVec.x) * Mathf.Rad2Deg) - 90f;
        handVisuals.transform.rotation = Quaternion.Euler(0f, 0f, zRot);

        // Rotate the sleeve to match the arm line
        angleVec = handLine.GetPosition(LINE_TOP_POINT) - handLine.GetPosition(LINE_BOTTOM_POINT);
        sleeve.localRotation = Quaternion.Euler(0f, 0f, (Mathf.Atan2(angleVec.y, angleVec.x) * Mathf.Rad2Deg) - 90f);
    }

    /// <summary>
    /// Set whether the boiling sprite animation should be playing on this hand
    /// </summary>
    public void SetBoil(bool isBoiling)
    {
        for (int i = 0; i < boilRenderers.Length; ++i)
        {
            boilRenderers[i].GetPropertyBlock(boilPropBlock);
            boilPropBlock.SetFloat(boilSnapID, isBoiling ? boilAnimationSpeed : 0f);
            boilRenderers[i].SetPropertyBlock(boilPropBlock);
        }
    }
}