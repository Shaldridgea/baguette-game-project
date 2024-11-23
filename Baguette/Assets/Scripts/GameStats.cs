using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic global data class to hold quantified statistics
/// </summary>
public abstract class GameStats<T>
{
    protected GameStats()
    {
#if UNITY_EDITOR
        instance = this;
#endif
        statsDictionary = new Dictionary<T, float>();
    }

    protected static Dictionary<T, float> statsDictionary;

#if UNITY_EDITOR
    protected static GameStats<T> instance;
    
    private static void PrintError(T wrongStat)
    {
        Debug.LogError("TrackingStat " + wrongStat.ToString() + " doesn't exist in " + instance.GetType().Name);
    }
#endif

    public static void SetStat(T statIndex, float newStat)
    {
        if (statsDictionary.ContainsKey(statIndex))
            statsDictionary[statIndex] = newStat;
#if UNITY_EDITOR
        else
            PrintError(statIndex);
#endif
    }

    public static void ModifyStat(T statIndex, float deltaStat)
    {
        if (statsDictionary.ContainsKey(statIndex))
            statsDictionary[statIndex] += deltaStat;
        
#if UNITY_EDITOR
        else
            PrintError(statIndex);
#endif
    }

    public static float GetStat(T statIndex)
    {
        if (statsDictionary.ContainsKey(statIndex))
            return statsDictionary[statIndex];
        else
        {
#if UNITY_EDITOR
            PrintError(statIndex);
#endif
            return float.NaN;
        }
    }
}

/// <summary>
/// Global data class to hold statistics on data pertaining to the player during a session
/// </summary>
public class PlayerStats : GameStats<Consts.PlayerTracking>
{
    public const float GoodwillLimit = 50f;

    public PlayerStats()
    {
        statsDictionary.Add(Consts.PlayerTracking.TotalBaguettes, 0f);
        statsDictionary.Add(Consts.PlayerTracking.Goodwill, -20f);
        statsDictionary.Add(Consts.PlayerTracking.Money, 100f);
        statsDictionary.Add(Consts.PlayerTracking.Debt, 1000f);
        statsDictionary.Add(Consts.PlayerTracking.Day, 0f);
#if UNITY_EDITOR
        statsDictionary[Consts.PlayerTracking.Money] = 1000f;
#endif
    }
}

/// <summary>
/// Global data class to hold statistics pertaining to what changed each day
/// </summary>
public class DayStats : GameStats<Consts.DayTracking>
{
    public DayStats()
    {
        statsDictionary.Add(Consts.DayTracking.BaguettesThisDay, 0f);
        statsDictionary.Add(Consts.DayTracking.BreadQuality, 0f);
    }
}