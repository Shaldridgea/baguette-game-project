using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Clock UI showing the in-game time
/// </summary>
public class ClockTimer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI[] clockTextBlocks;

    [SerializeField]
    private TextMeshProUGUI overtimeText;

    void Start()
    {
        DayTimeManager.Instance.MinuteEvent += UpdateTime;
        DayTimeManager.Instance.ShopCloseEvent += CheckForShopClose;

        FlowManager.Instance.StateStart += CheckForBakeStart;
        FlowManager.Instance.StateEnd += CheckForBakeEnd;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForBakeStart;
        FlowManager.Instance.StateEnd -= CheckForBakeEnd;
    }

    /// <summary>
    /// Update the clock display time
    /// </summary>
    private void UpdateTime(float time)
    {
        string twentyFourHrString = DayTimeManager.Instance.Current24Hour;
        for (int i = 0; i < 5; ++i)
            clockTextBlocks[i].SetText(twentyFourHrString[i].ToString());

        // Set last text block to AM/PM
        clockTextBlocks[^1].SetText(twentyFourHrString.Substring(6, 2));
    }

    private void CheckForShopClose(float time)
    {
        // Overtime starts when the shop closes
        overtimeText.gameObject.SetActive(true);
    }

    private void CheckForBakeStart(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        gameObject.SetActive(true);
        overtimeText.gameObject.SetActive(false);
    }

    private void CheckForBakeEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Baking)
            return;

        gameObject.SetActive(false);
    }
}