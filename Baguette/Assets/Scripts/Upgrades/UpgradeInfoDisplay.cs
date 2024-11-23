using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays all the info, levels, and selectable options for an upgrade
/// </summary>
public class UpgradeInfoDisplay : MonoBehaviour
{
    [SerializeField]
    private UpgradeBox upgradeBoxPrefab;

    [SerializeField]
    private UpgradeBox mainBox;

    public UpgradeData Upgrade { get; private set; }

    public int CurrentLevel { get; private set; }

    public int ProjectedLevel { get; private set; }

    public float ProjectedCost { get; private set; }

    private static UpgradesScreen screen;

    private List<UpgradeBox> selectionBoxes = new List<UpgradeBox>();

    public void PopulateUpgradeInfo(UpgradeData data, UpgradesScreen newScreen)
    {
        screen = newScreen;
        Upgrade = data;
        CurrentLevel = data.Level;

        mainBox.PopulateWithParameters(data.UpgradeName, null, string.Empty);
        int levelCheck = 0;
        UpgradeData.LevelValue value;
        UpgradeBox newBox;
        do
        {
            value = data.GetLevelValue(levelCheck);
            if (value == null)
                break;

            newBox = Instantiate(upgradeBoxPrefab, transform);
            newBox.PopulateWithUpgrade(data, levelCheck, this);
            selectionBoxes.Add(newBox);

            ++levelCheck;
        } while (value != null);
    }

    public void StoreUpgradeLevel(int upgradeLevel)
    {
        if(!Upgrade.CanGoBack)
            if (upgradeLevel <= CurrentLevel)
                return;

        // If the current projected level for this upgrade is
        // the level we're trying to store, then deselect it
        if(upgradeLevel == ProjectedLevel)
        {
            screen.RemovePotentialUpgrade(this);
            ResetUI();
            return;
        }

        ProjectedLevel = upgradeLevel;
        ProjectedCost = float.Parse(Upgrade.GetLevelValue(upgradeLevel).LevelCost);
        screen.StorePotentialUpgrade(this);
        for (int i = 0; i < selectionBoxes.Count; ++i)
        {
            selectionBoxes[i].SetSelected(i == upgradeLevel);
        }
    }

    public void MakeCurrentFromProjected()
    {
        CurrentLevel = ProjectedLevel;
        Upgrade.SetLevel(CurrentLevel);
        ProjectedCost = 0;
        SetSelectionState();
    }

    public void UpdateUpgradeLevel(int newLevel)
    {
        ProjectedLevel = CurrentLevel = newLevel;
        Upgrade.SetLevel(newLevel);
    }

    public void ResetUI()
    {
        ProjectedLevel = CurrentLevel;
        ProjectedCost = 0;
        SetSelectionState();
    }

    private void SetSelectionState()
    {
        for (int i = 0; i < selectionBoxes.Count; ++i)
        {
            selectionBoxes[i].SetDisabled(!Upgrade.CanGoBack && i < CurrentLevel);
            selectionBoxes[i].SetSelected(false);
            if(i == CurrentLevel)
                selectionBoxes[i].SetActive(true);
        }
    }
}