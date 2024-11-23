using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data describing what an upgrade is and how its associated values can change
/// </summary>
[CreateAssetMenu(fileName = "Upgrade data", menuName = "Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [System.Serializable]
    public class LevelValue
    {
        [SerializeField]
        private string levelName;

        [SerializeField]
        private Sprite levelSprite;

        [SerializeField]
        private string levelCost;

        [SerializeField]
        [TextArea]
        private string tooltip;

        public string LevelName => levelName;

        public Sprite LevelSprite => levelSprite;

        public string LevelCost => levelCost;

        public string Tooltip => tooltip;

        [SerializeField]
        private List<string> levelUpgrades;

        public string GetValue(int i) => levelUpgrades[i];
    }

    [SerializeField]
    private string frontendName;

    public string UpgradeName => frontendName;

    [SerializeField]
    private List<string> backendNames;

    [SerializeField]
    private int startingLevel = -1;

    [SerializeField]
    private Consts.UpgradeCategory category;

    public Consts.UpgradeCategory Category => category;

    /// <summary>
    /// Whether this upgrade can be reverted to a lower level
    /// </summary>
    [SerializeField]
    [Tooltip("Can this upgrade be reverted to a lower level?")]
    private bool canGoBack;

    public bool CanGoBack => canGoBack;

    [SerializeField]
    private List<LevelValue> levelValues;

    private int upgradeLevel;

    public int Level => upgradeLevel;

    public bool IsActive => upgradeLevel >= 0;

    private UpgradeValue[] upgradeValues;

    private void OnEnable()
    {
        upgradeLevel = startingLevel;
        if (backendNames == null)
            return;

        // Array matches the number of values expected and entries
        // are null until the upgrade value is registered with us
        upgradeValues = new UpgradeValue[backendNames.Count];
    }

    public bool TryRegisterValue(UpgradeValue upgradeVal)
    {
        bool success = false;
        int index = backendNames.FindIndex(0, n => n == upgradeVal.BackendName);
        if (index > -1)
        {
            success = true;
            upgradeValues[index] = upgradeVal;
            upgradeVal.IsRegistered = true;
        }

        return success;
    }

    public bool TryDeregisterValue(UpgradeValue upgradeVal)
    {
        bool wasNulled = false;
        for(int i = 0; i < upgradeValues.Length; ++i)
        {
            if (upgradeValues[i] == upgradeVal)
            {
                upgradeValues[i] = null;
                wasNulled = true;
            }
        }
        if (wasNulled)
            upgradeVal.IsRegistered = false;

        return wasNulled;
    }

    public void TryUpdateValue(string valueName)
    {
        if (upgradeLevel < 0)
            return;

        int index = backendNames.FindIndex(0, n => n == valueName);
        if (index < 0)
            return;

        upgradeValues[index].SetValue(levelValues[upgradeLevel].GetValue(index));
    }

    public void UpdateAllValues()
    {
        if (upgradeLevel < 0)
            return;

        for (int i = 0; i < backendNames.Count; ++i)
        {
            if (upgradeValues[i] == null)
                continue;

            upgradeValues[i].SetValue(levelValues[upgradeLevel].GetValue(i));
        }
    }

    public void IncreaseLevel() => upgradeLevel = Mathf.Min(++upgradeLevel, levelValues.Count-1);

    public void DecreaseLevel() => upgradeLevel = Mathf.Max(--upgradeLevel, 0);

    public void SetLevel(int newLevel) => upgradeLevel = Mathf.Clamp(newLevel, 0, levelValues.Count - 1);

    public LevelValue GetLevelValue(int level) => level < levelValues.Count ? levelValues[level] : null;
}