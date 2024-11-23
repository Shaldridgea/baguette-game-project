using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Calendar screen displaying a calendar page, along with debt display and paying logic
/// </summary>
public class CalendarScreen : MonoBehaviour
{
    [SerializeField]
    [Range(1, 12)]
    private int startMonth;

    [SerializeField]
    private RectTransform dayBoxesHolder;

    [SerializeField]
    private DayBox dayBoxPrefab;

    [SerializeField]
    private TextMeshProUGUI monthText;

    [SerializeField]
    private TextMeshProUGUI moneyText;

    [SerializeField]
    private TextMeshProUGUI debtText;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private Button payButton;

    private DateTime date;

    private bool cleanup;

    void Start()
    {
        FlowManager.Instance.StateStart += CheckForCalendarStart;
        FlowManager.Instance.StateEnd += CheckForCalendarEnd;

        date = new DateTime(2001, startMonth, 1);
        date = date.AddDays(PlayerStats.GetStat(Consts.PlayerTracking.Day));

        nextButton.onClick.AddListener(NextButtonClicked);
        payButton.onClick.AddListener(PayBillsButtonClicked);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForCalendarStart;
        FlowManager.Instance.StateEnd -= CheckForCalendarEnd;
    }

    private void CheckForCalendarStart(Consts.GameState newState)
    {
        // Cleanup occurs in a new state after the fade
        // when the calendar screen is no longer visible
        if (newState != Consts.GameState.Calendar)
        {
            if (cleanup)
            {
                for(int i = dayBoxesHolder.childCount-1; i >= 0; --i)
                {
                    Destroy(dayBoxesHolder.GetChild(i).gameObject);
                }
                gameObject.SetActive(false);
            }
            return;
        }

        gameObject.SetActive(true);

        string monthName = date.ToLongDateString().Split(' ')[1].Trim();
        monthText.text = monthName;
        // Create days on the calendar page
        for(int i = 0; i < DateTime.DaysInMonth(date.Year, date.Month); ++i)
        {
            DayBox box = Instantiate(dayBoxPrefab, dayBoxesHolder);
            box.SetDayNumber(i+1);
            box.SetCircleVisible(date.Day+1 == (i+1));
        }
        UpdateStickyNoteText();
    }

    private void CheckForCalendarEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Calendar)
            return;

        date = date.AddDays(1);
        PlayerStats.ModifyStat(Consts.PlayerTracking.Day, 1);
        cleanup = true;
    }

    private void NextButtonClicked()
    {
        FlowManager.Instance.Progress();
    }

    private void PayBillsButtonClicked()
    {
        // Pay off debt out of current money
        float money = PlayerStats.GetStat(Consts.PlayerTracking.Money);
        if (money >= 1f)
        {
            PlayerStats.ModifyStat(Consts.PlayerTracking.Debt, -money);
            PlayerStats.SetStat(Consts.PlayerTracking.Money, 0f);
            UpdateStickyNoteText();
        }
    }

    private void UpdateStickyNoteText()
    {
        moneyText.text = "Money: " + PlayerStats.GetStat(Consts.PlayerTracking.Money).ToString();
        debtText.text = "Debt: " + PlayerStats.GetStat(Consts.PlayerTracking.Debt);
    }
}