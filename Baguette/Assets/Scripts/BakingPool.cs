using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles trays of bread and tracks their baking progress
/// </summary>
public class BakingPool
{
    private List<GameObject> trayPoolList;

    private List<int> trayCookTimers;

    private int cookMinutesGoal;

    private bool useBakingSoda;

    public delegate void TrayTimerDelegate();

    public event TrayTimerDelegate TrayFinished;

    public int TrayCount => trayPoolList.Count;

    public bool BakingPaused;

    public BakingPool()
    {
        trayPoolList = new List<GameObject>();
        trayCookTimers = new List<int>();
        DayTimeManager.Instance.MinuteEvent += MinuteTickListener;
    }

    public void SetCookingTime(int cookMinutes) => cookMinutesGoal = cookMinutes;

    public void SetBakingPowder(bool isBakingSoda) => useBakingSoda = isBakingSoda;

    private void MinuteTickListener(float time)
    {
        if (BakingPaused)
            return;

        for (int i = 0; i < trayCookTimers.Count; ++i)
            if (++trayCookTimers[i] == cookMinutesGoal)
                TrayFinished?.Invoke();
    }

    public void AddTrayToPool(GameObject newTray)
    {
        trayPoolList.Add(newTray);
        trayCookTimers.Add(0);
        newTray.SetActive(false);
    }
    
    public void ClearAllTrays()
    {
        trayPoolList.Clear();
        trayCookTimers.Clear();
    }

    private GameObject RemoveTrayFromPool(int trayIndex)
    {
        GameObject removeTray = trayPoolList[trayIndex];
        trayPoolList.RemoveAt(trayIndex);
        trayCookTimers.RemoveAt(trayIndex);
        return removeTray;
    }

    public TrayBehaviour GetFinishedTray()
    {
        int index = trayCookTimers.FindIndex(0, i => i >= cookMinutesGoal);
        if (index < 0)
            return null;

        int trayCookTime = trayCookTimers[index];
        GameObject finishedTray = RemoveTrayFromPool(index);
        finishedTray.SetActive(true);

        TrayBehaviour trayBehaviour = finishedTray.GetComponentInChildren<TrayBehaviour>();
        trayBehaviour.SetOvercookTime(Mathf.Max(0, trayCookTime - cookMinutesGoal) * (useBakingSoda ? 2 : 1));
        return trayBehaviour;
    }
}