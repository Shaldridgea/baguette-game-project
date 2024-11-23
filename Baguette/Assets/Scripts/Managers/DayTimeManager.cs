using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tracks and fires events for the passage of time and days in-game
/// </summary>
public class DayTimeManager : MonoBehaviour
{
    #region Singleton
    public static DayTimeManager Instance { get; private set; }

    private DayTimeManager() { }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }
    #endregion

    [SerializeField]
    [Tooltip("Length of an in-game day in minutes")]
    private float dayLengthInMinutes;
    
    [SerializeField]
    [Tooltip("Number of work hours in a day")]
    private int workHoursCount;

    [SerializeField]
    [Tooltip("Number of hours of overtime you can do before the day is forced to end")]
    private int overtimeHoursCount;
    
    [SerializeField]
    [Tooltip("How many work hours pass till the shop opens")]
    private int hoursTillOpening;

    [SerializeField]
    [Tooltip("Time when the in-game day starts e.g. 7:00 AM")]
    private string startingDayTime;

#if UNITY_EDITOR
    [SerializeField]
    private float testingDayLength;
#endif

    private float dayLengthInSeconds;

    private float hourLengthInSeconds;

    public delegate void TimeDelegate(float timeInHours);

    public event TimeDelegate HourEvent;

    public event TimeDelegate MinuteEvent;

    public event TimeDelegate ShopOpenEvent;

    public event TimeDelegate ShopCloseEvent;

    private float currentTime;

    private float comparisonTime;

    private int comparisonHour;

    private int comparisonMinute;

    public int CurrentHour => (int)comparisonTime;

    public float CurrentTime => comparisonTime;

    public string Current24Hour => dayTime.ToString("HH:mm tt");
    public string Current12Hour => dayTime.ToString("hh:mm tt");

    public float CurrentTimePercentage => currentTime / dayLengthInSeconds;

    private DateTime dayTime;

    private bool paused = true;

    private const string START_DATE = "1 January 2001 ";

    void Start()
    {
        FlowManager.Instance.StateStart += CheckForBakeStart;
        FlowManager.Instance.StateEnd += CheckForBakeEnd;

        startingDayTime = START_DATE + startingDayTime;
        dayTime = DateTime.Parse(startingDayTime);

#if UNITY_EDITOR
        dayLengthInMinutes = testingDayLength;
#endif
        // Day length is in working hours, not with overtime
        dayLengthInSeconds = dayLengthInMinutes * 60f;
        hourLengthInSeconds = dayLengthInSeconds / workHoursCount;

        currentTime = 0f;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForBakeStart;
        FlowManager.Instance.StateEnd -= CheckForBakeEnd;
    }

    private void CheckForBakeStart(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        paused = false;
        dayTime = DateTime.Parse(startingDayTime);
        // In-game time is considered as starting at 0 every day
        currentTime = 0f;
        comparisonTime = 0f;
        comparisonMinute = 0;
        comparisonHour = 0;
    }

    private void CheckForBakeEnd(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        paused = true;
    }

    void Update()
    {
        if (paused)
            return;

        currentTime += Time.deltaTime;
        comparisonTime = currentTime / hourLengthInSeconds;

        dayTime = dayTime.AddMinutes(60f / hourLengthInSeconds * Time.deltaTime);
        // Check if our minutes or hours have progressed
        if (dayTime.Minute != comparisonMinute)
        {
            comparisonMinute = dayTime.Minute;

            if (CurrentHour > comparisonHour)
            {
                comparisonHour = CurrentHour;

                if (comparisonHour == hoursTillOpening)
                    ShopOpenEvent?.Invoke(CurrentTime);
                else
                if (comparisonHour == workHoursCount)
                    ShopCloseEvent?.Invoke(CurrentTime);
                else // Baking section is forcefully stopped when overtime finishes
                if (comparisonHour == workHoursCount + overtimeHoursCount)
                    FlowManager.Instance.Progress();

                HourEvent?.Invoke(CurrentTime);
            }

            MinuteEvent?.Invoke(CurrentTime);
        }
    }

    public void SetTime(int hour, int minute)
    {
        comparisonHour = hour;
        comparisonMinute = minute;

        currentTime = hourLengthInSeconds * hour;
        currentTime += (minute / 60f) * hourLengthInSeconds;

        dayTime = DateTime.Parse(startingDayTime);
        dayTime = dayTime.AddHours(hour);
        dayTime = dayTime.AddMinutes(minute);
    }
}