using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Simulates demand/customers buying product, and tracks how much product you've made to sell
/// </summary>
public class SupplyDemandManager : MonoBehaviour
{
    #region Singleton
    public static SupplyDemandManager Instance { get; private set; }

    private SupplyDemandManager() { }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    [SerializeField]
    private AnimationCurve demandIncreaseCurve;

    [SerializeField]
    [Tooltip("How often demand will increase in in-game hours")]
    private float demandIncrementInterval;

    [SerializeField]
    private int minimumDemandIncrease;

    [SerializeField]
    private int maximumDemandIncrease;

    [Header("Effects")]
    [SerializeField]
    private AudioClip doorOpenSound;

    private Dictionary<Consts.BreadType, int> breadSupply = new Dictionary<Consts.BreadType, int>();

    private Dictionary<Consts.BreadType, int> breadSold = new Dictionary<Consts.BreadType, int>();

    private int demandIncrease;

    /// <summary>
    /// Compares current supply and demand. Positive is how much supply, negative is how much demand
    /// </summary>
    public int SupplyDemandCounter { get { return supplyCounter - demandCounter; } }

    public int SoldCounter { get; private set; }

    private int supplyCounter;

    private int demandCounter;

    private float lastIncreaseTime;

    private bool isShopOpen;

    // Start is called before the first frame update
    void Start()
    {
        FlowManager.Instance.StateStart += CheckForBakeStart;
        DayTimeManager.Instance.ShopOpenEvent += CheckForShopOpen;
        DayTimeManager.Instance.ShopCloseEvent += CheckForShopClose;
        for (Consts.BreadType i = 0; i < Consts.BreadType.Last; ++i)
        {
            breadSupply.Add(i, 0);
            breadSold.Add(i, 0);
        }

        // Increments of 1 are an hour, so 0.5 would be half an hour etc.
        if (demandIncrementInterval % 1 != 0f)
            DayTimeManager.Instance.MinuteEvent += IncreaseDemandCheck;
        else
            DayTimeManager.Instance.HourEvent += IncreaseDemandCheck;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForBakeStart;
    }

    private void CheckForShopOpen(float time) => isShopOpen = true;
    private void CheckForShopClose(float time) => isShopOpen = false;

    private void CheckForBakeStart(Consts.GameState newState)
    {
        if (newState != Consts.GameState.Baking)
            return;

        // Track our total baguettes from the previous day
        PlayerStats.ModifyStat(Consts.PlayerTracking.TotalBaguettes, DayStats.GetStat(Consts.DayTracking.BaguettesThisDay));
        
        DayStats.SetStat(Consts.DayTracking.BaguettesThisDay, 0);
        DayStats.SetStat(Consts.DayTracking.BreadQuality, 0f);
        for (Consts.BreadType i = 0; i < Consts.BreadType.Last; ++i)
        {
            // Don't reset bread supply to keep surplus from previous days
            breadSold[i] = 0;
        }
        SoldCounter = 0;

        demandCounter = 0;

        // Demand increases as the goodwill you earn increases
        demandIncrease = (int)Mathf.Lerp(minimumDemandIncrease, maximumDemandIncrease,
            (PlayerStats.GetStat(Consts.PlayerTracking.Goodwill) + PlayerStats.GoodwillLimit) / (PlayerStats.GoodwillLimit * 2f));

        lastIncreaseTime = 0f;
        isShopOpen = false;
    }

    private void IncreaseDemandCheck(float hoursTime)
    {
        if (hoursTime >= lastIncreaseTime + demandIncrementInterval)
            lastIncreaseTime += demandIncrementInterval;
        else
            return;

        if (!isShopOpen)
            return;
        
        // Demand becomes larger through the day, being at peak during midday, then petering off toward closing.
        // Gets the curve demand along with the general demand increase to get how much bread someone will buy now
        IncreaseDemand((int)(demandIncreaseCurve.Evaluate(DayTimeManager.Instance.CurrentTimePercentage) * demandIncrease));
        AudioPlayer.PlayOnceFree(doorOpenSound, Consts.AudioLocation.Up);
    }

    public void IncreaseSupply(int supplyIncrease, Consts.BreadType type)
    {
        DayStats.ModifyStat(Consts.DayTracking.BaguettesThisDay, supplyIncrease);
        breadSupply[type] += supplyIncrease;
        supplyCounter += supplyIncrease;
        UpdateSoldBread();
        Debug.Log("supply increased: " + supplyIncrease);
    }

    public void IncreaseDemand(int demandUp)
    {
        demandCounter += demandUp;
        UpdateSoldBread();
        Debug.Log("demand increased: " + demandUp);
    }

    private void UpdateSoldBread()
    {
        // Sell bread until there is no demand left.
        // Prefer simpler/less fancy bread first
        while (demandCounter > 0)
        {
            bool breadAvailable = false;
            for (Consts.BreadType i = 0; i < Consts.BreadType.Last; ++i)
            {
                if (breadSupply[i] > 0)
                {
                    ++breadSold[i];
                    --breadSupply[i];
                    ++SoldCounter;
                    --demandCounter;
                    --supplyCounter;
                    if (breadSupply[i] > 0)
                        breadAvailable = true;
                }
            }
            if (!breadAvailable)
                break;
        }
    }

    public int GetBreadSold(Consts.BreadType type) => breadSold[type];
}