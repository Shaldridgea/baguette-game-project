using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bell object that can be touched to end the day when the shop is closed
/// </summary>
public class EndBell : MonoBehaviour
{
    [SerializeField]
    private Touchable bellTouch;

    [SerializeField]
    private Vector3 startPos;

    [SerializeField]
    private Vector3 endPos;

    [SerializeField]
    private AudioClip dingSound;

    void Start()
    {
        bellTouch.InteractEvent += BellInteract;
        DayTimeManager.Instance.ShopCloseEvent += ShopCloseBell;
        FlowManager.Instance.StateStart += CheckForBakeStart;

        // Bell should already be visible to end the day if in practice
        // or if time is set to start after closing
        if (TutorialController.InPracticeMode || DayTimeManager.Instance.CurrentTimePercentage >= 1f)
            transform.localPosition = endPos;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForBakeStart;
    }

    private void CheckForBakeStart(Consts.GameState state)
    {
        if (state != Consts.GameState.Baking)
            return;

        transform.localPosition = TutorialController.InPracticeMode ? endPos : startPos;
    }

    private void ShopCloseBell(float timeInHours)
    {
        // Animate the bell into view
        LeanTween.moveLocalX(gameObject, endPos.x, 1f).setEase(LeanTweenType.easeOutQuad);
    }

    private void BellInteract(PlayerHand hand)
    {
        FlowManager.Instance.Progress();
        AudioPlayer.PlayOnceFree(dingSound, Consts.AudioLocation.Up);
    }
}