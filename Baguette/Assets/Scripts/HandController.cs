using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the movement and interaction of the hands
/// </summary>
public class HandController : MonoBehaviour
{
    [SerializeField]
    private PlayerHand handPrefab;

    [SerializeField]
    private string[] activateInputNames;

    [SerializeField]
    private string[] deactivateInputNames;

    [SerializeField]
    private string interactPress;

    [SerializeField]
    private Vector3 screenLimits;

    private PlayerHand[] hands;

    private float sensitivity;

    private bool AreHandsActive => isBakingStateActive && Cursor.lockState == CursorLockMode.Locked;

    private bool isBakingStateActive;

    private const int LEFT = 0;

    private const int RIGHT = 1;

    void Start()
    {
        hands = new PlayerHand[activateInputNames.Length];

        // Create and position hands
        hands[LEFT] = Instantiate(handPrefab, transform, false);
        hands[LEFT].transform.localPosition = -Vector3.right * 2f;
        hands[LEFT].FlipSprites = true;

        hands[RIGHT] = Instantiate(handPrefab, transform, false);
        hands[RIGHT].transform.localPosition = Vector3.right * 2f;

        hands[LEFT].name = "Left hand";
        hands[RIGHT].name = "Right hand";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        sensitivity = Camera.main.orthographicSize * 2 / Screen.height;

        InputManager.Instance.InputEvent += HandActiveInput;
        InputManager.Instance.InputEvent += InteractInput;

        FlowManager.Instance.StateStart += CheckForBakeStart;
        FlowManager.Instance.StateEnd += CheckForBakeEnd;
        FlowManager.Instance.PauseEvent += CheckForPause;
        FlowManager.Instance.UnpauseEvent += CheckForUnpause;
    }

    private void OnDestroy()
    {
        InputManager.Instance.InputEvent -= HandActiveInput;
        InputManager.Instance.InputEvent -= InteractInput;

        FlowManager.Instance.StateStart -= CheckForBakeStart;
        FlowManager.Instance.StateEnd -= CheckForBakeEnd;
        FlowManager.Instance.PauseEvent -= CheckForPause;
        FlowManager.Instance.UnpauseEvent -= CheckForUnpause;
    }

    private void CheckForBakeStart(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isBakingStateActive = true;
    }

    private void CheckForBakeEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Baking)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isBakingStateActive = false;
    }

    private void CheckForPause(Consts.GameState newState)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CheckForUnpause(Consts.GameState newState)
    {
        if (!isBakingStateActive)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!AreHandsActive)
            return;

        Vector3 handMovement = InputManager.Instance.MouseDelta * sensitivity;
        for (int i = 0; i < hands.Length; ++i)
        {
            // Keep our hands in the bounds of the screen
            if (hands[i].InUse)
            {
                Vector3 clampedMovement = handMovement;
                Vector3 nextPos = hands[i].transform.localPosition + handMovement;
                if (nextPos.x <= -screenLimits.x)
                    clampedMovement.x += (-screenLimits.x) - nextPos.x;
                if (nextPos.x >= screenLimits.x)
                    clampedMovement.x += screenLimits.x - nextPos.x;
                if(nextPos.y <= -screenLimits.y)
                    clampedMovement.y += (-screenLimits.y) - nextPos.y;
                if(nextPos.y >= screenLimits.y)
                    clampedMovement.y += screenLimits.y - nextPos.y;

                hands[i].Move(clampedMovement);
            }
        }
    }

    private void HandActiveInput(string inputID)
    {
        for (int i = LEFT; i <= RIGHT; ++i)
        {
            // Hand has been activated
            if (inputID == activateInputNames[i])
            {
                if (!AreHandsActive)
                    continue;

                hands[i].InUse = true;
                hands[i].SetBoil(true);
                HandDropTouchable(i);
            }
            else
            // Hand has been deactivated
            if (inputID == deactivateInputNames[i])
            {
                hands[i].InUse = false;
                hands[i].SetBoil(false);
                HandDropTouchable(i);
            }
        }
    }

    private void HandDropTouchable(int handIndex)
    {
        if (CameraController.IsRotating)
            return;

        // Drop anything the hands are holding.
        // Two handed objects are attached to the right hand only,
        // so here we also check if the left hand let go and
        // whether the right hand needs to let go of something
        if (hands[handIndex].IsCarrying)
            hands[handIndex].TryDetachTouchable();
        else if (handIndex == LEFT && hands[RIGHT].IsCarrying)
            if (hands[RIGHT].CarryingTouchable.RequireTwoHands)
                hands[RIGHT].TryDetachTouchable();
    }

    private void InteractInput(string inputID)
    {
        if (!AreHandsActive)
            return;

        if (CameraController.IsRotating)
            return;

        if (inputID != interactPress)
            return;

        Touchable[] touches = new Touchable[2];
        Touchable thisFrameDropped = null;

        for(int i = 0; i < hands.Length; ++i)
        {
            if (!hands[i].InUse)
                continue;

            if(hands[i].IsCarrying)
            {
                thisFrameDropped = hands[i].CarryingTouchable;
                hands[i].TryDetachTouchable();
                continue;
            }

            touches[i] = hands[i].FindTouchable();
        }

        // Things being carried are still counted as being touchable.
        // Ignore things we're touching if we just dropped them, so that
        // two hands over an object that's dropped won't pick it back up instantly
        if (touches[LEFT] == thisFrameDropped)
            touches[LEFT] = null;

        if (touches[RIGHT] == thisFrameDropped)
            touches[RIGHT] = null;

        // If we're picking up the same thing with both
        // hands, then we prefer the right hand to attach it to.
        // This also handles two handed objects being only right attached
        if(touches[LEFT] && touches[RIGHT])
            if(touches[LEFT] == touches[RIGHT])
            {
                hands[RIGHT].InteractTouchable(touches[LEFT]);
                return;
            }

        // Pick up individually as long as they're not two handed
        if(touches[LEFT] && !touches[LEFT].RequireTwoHands)
            hands[LEFT].InteractTouchable(touches[LEFT]);

        if (touches[RIGHT] && !touches[RIGHT].RequireTwoHands)
            hands[RIGHT].InteractTouchable(touches[RIGHT]);
    }
}