using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dough logic for the dough item
/// </summary>
public class DoughBehaviour : MonoBehaviour
{
    [SerializeField]
    private TouchableDrag doughDrag;

    [SerializeField]
    private LayerMask doughModifyMask;

    void Start()
    {
        doughDrag.DropEvent += DropCheckForTray;
    }

    public void DropCheckForTray(PlayerHand hand)
    {
        // Find a tray to put this dough on
        Collider2D trayCollider = CollisionHandler.CheckPositionForCollider(transform.position - Vector3.up, doughModifyMask, "Tray");
        if (trayCollider != null)
        {
            // Don't add ourselves to tray if no spaces left
            if(!TrayBehaviour.GetCached(trayCollider.gameObject).IsEmptySpaceAvailable)
            {
                doughDrag.Fall();
                return;
            }

            CollisionHandler.AttachToObject(transform, trayCollider.transform);
            LeanTween.cancel(gameObject);
            // Remove the draggable component from ourselves
            // when on the tray so we can't be removed
            Destroy(doughDrag);
        }
    }
}