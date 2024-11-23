using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RotaryHeart.Lib.SerializableDictionary;

/// <summary>
/// End of day results display screen
/// </summary>
public class ResultsScreen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI resultsTextPrefab;

    [SerializeField]
    private Button nextButton;

    [SerializeField]
    private RectTransform resultsTextLayout;

    [SerializeField]
    private Slider goodwillBar;

    [SerializeField]
    private BreadTypeFloatDictionary breadPriceTable;

    private List<TextMeshProUGUI> texts;

    private bool resultsCleanup;

    [System.Serializable]
    public class BreadTypeFloatDictionary : SerializableDictionaryBase<Consts.BreadType, float> { }

    private void Start()
    {
        FlowManager.Instance.StateStart += CheckForStartResults;
        FlowManager.Instance.StateEnd += CheckForEndResults;
        nextButton.onClick.AddListener(FlowManager.Instance.Progress);
        texts = new List<TextMeshProUGUI>(2);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForStartResults;
        FlowManager.Instance.StateEnd -= CheckForEndResults;
    }

    private void CheckForStartResults(Consts.GameState newState)
    {
        // Cleanup occurs in a new state after the fade
        // when the results screen is no longer visible
        if (newState != Consts.GameState.Results)
        {
            if(resultsCleanup)
            {
                resultsCleanup = false;

                for (int i = 0; i < texts.Count; ++i)
                    Destroy(texts[i].gameObject);
                texts.Clear();
                gameObject.SetActive(false);
            }
            return;
        }

        float moneyMade = 0f;
        if(!TutorialController.InPracticeMode)
            CalculateResults(ref moneyMade);
        
        gameObject.SetActive(true);
        if (TutorialController.InPracticeMode)
        {
            AddResultsText($"Baguettes made: {DayStats.GetStat(Consts.DayTracking.BaguettesThisDay)}");
            goodwillBar.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            AddResultsText($"Baguettes sold: {SupplyDemandManager.Instance.SoldCounter}");
            string moneyString = Consts.MoneyToString(moneyMade);
            AddResultsText($"Profits: € {moneyString}");
            goodwillBar.transform.parent.gameObject.SetActive(true);
        }

        goodwillBar.transform.parent.SetAsLastSibling();
        goodwillBar.value = PlayerStats.GetStat(Consts.PlayerTracking.Goodwill) / PlayerStats.GoodwillLimit;
    }

    private void CheckForEndResults(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Results)
            return;

        resultsCleanup = true;
    }

    private void AddResultsText(string newText)
    {
        texts.Add(Instantiate(resultsTextPrefab, resultsTextLayout));
        texts[^1].SetText(newText);
    }

    private void CalculateResults(ref float profitsToday)
    {
        // Village goodwill
        float goodwillMaxChange = PlayerStats.GoodwillLimit - Mathf.Abs(PlayerStats.GetStat(Consts.PlayerTracking.Goodwill));
        float unservedCustomers = Mathf.Min(SupplyDemandManager.Instance.SupplyDemandCounter, 0);
        float clampedQuality = Mathf.Clamp(DayStats.GetStat(Consts.DayTracking.BreadQuality) + unservedCustomers, -goodwillMaxChange, goodwillMaxChange);
        PlayerStats.ModifyStat(Consts.PlayerTracking.Goodwill, clampedQuality);

        // Money totalling
        for (Consts.BreadType i = 0; i < Consts.BreadType.Last; ++i)
            profitsToday += SupplyDemandManager.Instance.GetBreadSold(i) * breadPriceTable[i];
        PlayerStats.ModifyStat(Consts.PlayerTracking.Money, profitsToday);
    }
}