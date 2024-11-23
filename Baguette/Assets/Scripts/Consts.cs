using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consts
{
    public enum BreadType
    {
        Normal,
        Cheese,
        Sesame,
        Chocolate,
        Last
    }

    public enum GameState
    {
        MainMenu,
        Baking,
        Results,
        Upgrades,
        Calendar,
        Last
    }

    public enum PlayerTracking
    {
        TotalBaguettes,
        Goodwill,
        Money,
        Debt,
        Day
    }

    public enum DayTracking
    {
        BaguettesThisDay,
        BreadQuality
    }

    public enum UpgradeCategory
    {
        Equipment,
        Ingredients,
        Last
    }

    public enum AudioLocation
    {
        Default,
        Middle,
        Left,
        Right,
        Up,
        Music
    }

    public const string MAIN_MENU_SCENE = "TitleScene";
    public const string PLAY_SCENE = "MainScene";

    public const int TOUCHABLE_MASK = 1 << 9;
    public const int SCREEN_MASK = 1 << 10;
    public const int DROP_AREA_MASK = 1 << 12;

    /// <summary>
    /// Returns prettified string version of money, limiting decimal places for non-whole numbers
    /// </summary>
    public static string MoneyToString(float money) => money % 1 != 0f ? money.ToString("F2") : money.ToString();
}