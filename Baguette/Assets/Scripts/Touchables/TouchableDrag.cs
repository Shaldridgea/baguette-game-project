using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Touchable object that can be dragged around by the player's hands
/// </summary>
public class TouchableDrag : Touchable
{
    [SerializeField]
    private GameObject twoHandedIcon;

    [SerializeField]
    private bool matchHandRotation;

    [SerializeField]
    [Tooltip("Collision layers this object can be dropped on")]
    private LayerMask dropMask;

    [SerializeField]
    [Tooltip("Tags for objects this can be dropped on")]
    private List<string> dropTags;

    public bool MatchHandRotation { get { return matchHandRotation; } set { matchHandRotation = value; } }

    public LayerMask DropMask => dropMask;

    public string[] DropTags => dropTags.ToArray();

    public event TouchableDelegate HoldEvent;

    public event TouchableDelegate DropEvent;

    private const float FALL_SPEED = 5f;

    private int handEnterCounter = 0;

    public override void InteractHand(PlayerHand hand)
    {
        base.InteractHand(hand);

        hand.AttachTouchable(this);
        if(requireTwoHands)
            twoHandedIcon.SetActive(false);
    }

    public override void EnterHand(PlayerHand hand)
    {
        base.EnterHand(hand);
        if (requireTwoHands)
        {
            twoHandedIcon.SetActive(true);
            ++handEnterCounter;
        }
    }

    public override void LeaveHand(PlayerHand hand)
    {
        base.LeaveHand(hand);
        if (requireTwoHands)
        {
            if(--handEnterCounter == 0)
                twoHandedIcon.SetActive(false);
        }
    }

    public void Hold(PlayerHand hand)
    {
        HoldEvent?.Invoke(hand);
    }

    public void Drop(PlayerHand hand)
    {
        DropEvent?.Invoke(hand);
    }

    /// <summary>
    /// Make this object fall onto something or off-screen, with an animation
    /// </summary>
    public void Fall()
    {
        float yGoal = -15f;
        float distance = 15f;
        float moveTime = 0.8f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 10f, Consts.SCREEN_MASK);
        // Set where to fall if we have something to fall onto
        if(hit)
        {
            yGoal = hit.point.y - 0.4f;
            distance = hit.distance;
            moveTime = Mathf.Min(distance / FALL_SPEED, 0.6f);
        }

        // Change our tween depending on if it's
        // falling onto something or fall off-screen
        LTDescr fallTween = LeanTween.moveLocalY(gameObject, yGoal, moveTime);
        if (!hit)
        {
            fallTween.setEase(LeanTweenType.easeInBack);
            fallTween.setOnComplete(OnFallComplete);
        }
        else
            fallTween.setEase(LeanTweenType.easeOutBounce);
    }

    private void OnFallComplete()
    {
        PlayerTool tool = GetComponent<PlayerTool>();
        if (tool != null)
            tool.Replace();

        Destroy(gameObject);
    }
}