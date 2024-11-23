using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic class for objects that can be touched by the player's hands
/// </summary>
public class Touchable : CacheBehaviour<Touchable>
{
    [SerializeField]
    protected bool requireTwoHands;

    private const int LAYER_ORDER_GAP = 100;

    private SpriteRenderer spriteRend;

    public bool RequireTwoHands { get { return requireTwoHands; } set { requireTwoHands = value; } }

    public int TouchableSortOrder { get { return spriteRend.sortingLayerID * (LAYER_ORDER_GAP * 2) + spriteRend.sortingOrder; } }

    public delegate void TouchableDelegate(PlayerHand hand);

    public event TouchableDelegate InteractEvent;

    public event TouchableDelegate HandEnterEvent;

    public event TouchableDelegate HandLeaveEvent;

    protected void Start()
    {
        spriteRend = GetComponent<SpriteRenderer>();
        if(!spriteRend)
            spriteRend = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void InteractHand(PlayerHand hand)
    {
        Debug.Log("Interacted with by " + hand.name);
        InteractEvent?.Invoke(hand);
    }

    public virtual void EnterHand(PlayerHand hand)
    {
        Debug.Log("Entered by " + hand.name);
        HandEnterEvent?.Invoke(hand);
    }

    public virtual void LeaveHand(PlayerHand hand)
    {
        Debug.Log("Left by " + hand.name);
        HandLeaveEvent?.Invoke(hand);
    }
}