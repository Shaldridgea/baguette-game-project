using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Oven logic controlling its display and interaction, along with data for bread cooking
/// </summary>
public class OvenBehaviour : MonoBehaviour
{
    [SerializeField]
    private Touchable openedDoorTouch;

    [SerializeField]
    private Touchable closedDoorTouch;

    [SerializeField]
    private Collider2D ovenCollider;

    [SerializeField]
    private UpgradeValueInt maxTrayCapacity;

    [SerializeField]
    private UpgradeValueInt breadCookMinutesLength;

    [SerializeField]
    private UpgradeValueBool usingSlowDough;

    [Header("Effects")]
    [SerializeField]
    private TextMeshPro trayCapacityText;

    [SerializeField]
    private SpriteRenderer trayReadyLight;

    [SerializeField]
    private Sprite[] ovenLightSprites;

    [SerializeField]
    private AudioClip bakingFinishSound;

    [SerializeField]
    private Transform smellFill;

    private BakingPool bakingPool;

    private bool doorOpen;

    private int finishedTrayCount;

    private LTDescr smellMoveTween;

    private Vector3 smellFillStart;

    void Start()
    {
        maxTrayCapacity.Init();
        breadCookMinutesLength.Init();
        usingSlowDough.Init();
        bakingPool = new BakingPool();

        bakingPool.TrayFinished += BreadFinishedBaking;
        openedDoorTouch.InteractEvent += ToggleDoor;
        closedDoorTouch.InteractEvent += ToggleDoor;
        closedDoorTouch.InteractEvent += CheckForFinishedTray;

        FlowManager.Instance.StateStart += CheckForBakeStart;
        smellFillStart = smellFill.localPosition;
    }

    private void OnDestroy()
    {
        maxTrayCapacity.Deregister();
        breadCookMinutesLength.Deregister();
        usingSlowDough.Deregister();
        FlowManager.Instance.StateStart -= CheckForBakeStart;
    }

    private void CheckForBakeStart(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        bakingPool.ClearAllTrays();
        bakingPool.SetCookingTime(breadCookMinutesLength);
        bakingPool.SetBakingPowder(usingSlowDough);
        finishedTrayCount = 0;

        doorOpen = true;
        ToggleDoor(null);
        UpdateOvenDisplay();
    }

    private void BreadFinishedBaking()
    {
        ++finishedTrayCount;
        UpdateOvenDisplay();
        AudioPlayer.PlayOnceFree(bakingFinishSound, Consts.AudioLocation.Left);
        smellFill.localPosition = smellFillStart;
        smellMoveTween = LeanTween.moveLocal(smellFill.gameObject, new Vector3(5.52f, 6.12f), 2f).setLoopClamp();
    }

    private void CheckForFinishedTray(PlayerHand hand)
    {
        TrayBehaviour finishedTray = bakingPool.GetFinishedTray();
        if(finishedTray != null)
        {
            finishedTray.ConvertDoughToBread();
            finishedTray.TrayDrag.transform.position = hand.transform.position - Vector3.right;
            hand.AttachTouchable(finishedTray.TrayDrag);

            --finishedTrayCount;
            UpdateOvenDisplay();

            if (smellMoveTween != null)
                smellMoveTween.setLoopClamp(1);
        }
    }

    private void ToggleDoor(PlayerHand hand)
    {
        doorOpen = !doorOpen;
        openedDoorTouch.gameObject.SetActive(doorOpen);
        closedDoorTouch.gameObject.SetActive(!doorOpen);
        ovenCollider.enabled = doorOpen;
        bakingPool.BakingPaused = doorOpen;
    }

    public bool TryConsumeTray(GameObject newTray)
    {
        bool canAdd = bakingPool.TrayCount < maxTrayCapacity;
        if (canAdd)
        {
            bakingPool.AddTrayToPool(newTray);
            UpdateOvenDisplay();
        }

        return canAdd;
    }

    private void UpdateOvenDisplay()
    {
        // Show empty spaces as a number forced to two places
        // with a zero at the start if necessary
        trayCapacityText.text = (maxTrayCapacity - bakingPool.TrayCount).ToString("D2");
        trayReadyLight.sprite = ovenLightSprites[Mathf.Min(finishedTrayCount, 1)];
    }
}