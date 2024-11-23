using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Screen for display and control of changing and buying upgrades
/// </summary>
public class UpgradesScreen : MonoBehaviour
{
    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button nextButton;

    [Header("Layout elements")]
    [SerializeField]
    private RectTransform buttonTabsLayout;

    [SerializeField]
    private RectTransform scrollContent;

    [Header("UI Prefabs")]
    [SerializeField]
    private UpgradeInfoDisplay upgradeInfoPrefab;

    [SerializeField]
    private Button offsetFillButtonPrefab;

    [Header("Cost texts")]
    [SerializeField]
    private TextMeshProUGUI moneyText;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private TextMeshProUGUI totalText;

    [SerializeField]
    private Color normalTotalColour;

    [SerializeField]
    private Color invalidTotalColour;

    private bool upgradeScreenCleanup;

    private List<UpgradeData> allUpgradesList;

    private Dictionary<Consts.UpgradeCategory, List<UpgradeInfoDisplay>> categoryUpgradesDict = new Dictionary<Consts.UpgradeCategory, List<UpgradeInfoDisplay>>();

    private List<UpgradeInfoDisplay> projectedUpgrades = new List<UpgradeInfoDisplay>();

    private Consts.UpgradeCategory currentCategory;

    private float projectedUpgradeCost = 0;

    void Start()
    {
        FlowManager.Instance.StateStart += CheckForUpgradeStart;
        FlowManager.Instance.StateEnd += CheckForUpgradeEnd;
        confirmButton.onClick.AddListener(ConfirmButtonClicked);
        nextButton.onClick.AddListener(NextButtonClicked);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForUpgradeStart;
        FlowManager.Instance.StateEnd -= CheckForUpgradeEnd;
    }

    public void SetUpgradeList(List<UpgradeData> upgrades)
    {
        if (allUpgradesList != null)
            return;
        
        allUpgradesList = upgrades;
        for (Consts.UpgradeCategory i = 0; i < Consts.UpgradeCategory.Last; ++i)
            categoryUpgradesDict.Add(i, new List<UpgradeInfoDisplay>());
    }

    private void CheckForUpgradeStart(Consts.GameState newState)
    {
        // Cleanup occurs in a new state after the fade
        // when the upgrades screen is no longer visible
        if (newState != Consts.GameState.Upgrades)
        {
            if (upgradeScreenCleanup)
            {
                upgradeScreenCleanup = false;
                gameObject.SetActive(false);
            }
            return;
        }

        gameObject.SetActive(true);

        // Upgrade screen UI only needs to be created once
        if(scrollContent.childCount == 0)
        {
            UpgradeInfoDisplay upgradeDisplay = null;
            for (int i = 0; i < allUpgradesList.Count; ++i)
            {
                upgradeDisplay = Instantiate(upgradeInfoPrefab, scrollContent);
                categoryUpgradesDict[allUpgradesList[i].Category].Add(upgradeDisplay);
                upgradeDisplay.PopulateUpgradeInfo(allUpgradesList[i], this);
                upgradeDisplay.ResetUI();
            }

            Button tabButton = null;
            for (Consts.UpgradeCategory c = 0; c < Consts.UpgradeCategory.Last; ++c)
            {
                // 'ts' variable is local variable capture for the button lambda,
                // otherwise they'd all get overwritten with the final value
                Consts.UpgradeCategory ts = c;
                tabButton = Instantiate(offsetFillButtonPrefab, buttonTabsLayout);
                tabButton.onClick.AddListener(() => SetCurrentCategory(ts));
                tabButton.GetComponentInChildren<TextMeshProUGUI>().text = c.ToString();
            }
        }
        MakeCurrentCategoryVisible();

        UpdateCalculationUI(true, true, true);
    }

    private void CheckForUpgradeEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Upgrades)
            return;

        upgradeScreenCleanup = true;
        UpgradeManager.Instance.UpdateAllUpgrades();
        foreach(UpgradeInfoDisplay d in projectedUpgrades)
            d.ResetUI();
        projectedUpgrades.Clear();
    }

    private void MakeCurrentCategoryVisible()
    {
        for(Consts.UpgradeCategory c = 0; c < Consts.UpgradeCategory.Last; ++c)
        {
            List<UpgradeInfoDisplay> upgradeDisplays = categoryUpgradesDict[c];

            bool isCurrent = c == currentCategory;
            for (int i = 0; i < upgradeDisplays.Count; ++i)
                upgradeDisplays[i].gameObject.SetActive(isCurrent);
        }
    }

    private void SetCurrentCategory(Consts.UpgradeCategory newCategory)
    {
        if (newCategory == currentCategory)
            return;

        currentCategory = newCategory;
        MakeCurrentCategoryVisible();
    }

    public void StorePotentialUpgrade(UpgradeInfoDisplay display)
    {
        if (!projectedUpgrades.Contains(display))
            projectedUpgrades.Add(display);

        UpdateCostCalculation();
    }

    public void RemovePotentialUpgrade(UpgradeInfoDisplay display)
    {
        projectedUpgrades.Remove(display);

        UpdateCostCalculation();
    }

    private void UpdateCostCalculation()
    {
        projectedUpgradeCost = 0;
        foreach (UpgradeInfoDisplay d in projectedUpgrades)
            projectedUpgradeCost += d.ProjectedCost;

        UpdateCalculationUI(false, true, true);
    }

    private void NextButtonClicked()
    {
        FlowManager.Instance.Progress();
    }

    private void ConfirmButtonClicked()
    {
        if (PlayerStats.GetStat(Consts.PlayerTracking.Money) - projectedUpgradeCost < 0)
            return;

        foreach(UpgradeInfoDisplay d in projectedUpgrades)
            d.MakeCurrentFromProjected();

        PlayerStats.ModifyStat(Consts.PlayerTracking.Money, -projectedUpgradeCost);
        projectedUpgradeCost = 0;
        UpdateCalculationUI(true, true, true);
        projectedUpgrades.Clear();
    }

    private void UpdateCalculationUI(bool updateMoney, bool updateCost, bool updateTotal)
    {
        float m = PlayerStats.GetStat(Consts.PlayerTracking.Money);
        if (updateMoney)
            moneyText.text = Consts.MoneyToString(m);

        if (updateCost)
            costText.text = projectedUpgradeCost.ToString();

        if (updateTotal)
        {
            float t = m - projectedUpgradeCost;
            totalText.color = t < 0 ? invalidTotalColour : normalTotalColour;
            totalText.text = Consts.MoneyToString(t);
        }
    }
}