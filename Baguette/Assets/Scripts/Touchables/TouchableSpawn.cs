using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Touchable object that spawns something to attach to the player's hand
/// </summary>
public class TouchableSpawn : Touchable
{
    [SerializeField]
    private GameObject spawnObject;

    [SerializeField]
    private bool attachToHand;

    [SerializeField]
    private Vector3 spawnOffset;

    public override void InteractHand(PlayerHand hand)
    {
        base.InteractHand(hand);
        Transform spawn = Instantiate(spawnObject, transform.position, Quaternion.identity).transform;
        if (attachToHand)
        {
            spawn.position = hand.transform.position + spawnOffset;
            hand.AttachTouchable(spawn.GetComponent<Touchable>());
        }
    }
}