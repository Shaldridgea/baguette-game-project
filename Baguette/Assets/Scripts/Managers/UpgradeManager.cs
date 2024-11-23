using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles registering and updating upgrades and their values 
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    #region Singleton
    public static UpgradeManager Instance { get; private set; }

    private UpgradeManager() { }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    #endregion

    private void OnDestroy()
    {
        for (int i = allValues.Count-1; i >= 0; --i)
            DeregisterUpgradeValue(allValues[i]);
    }

    private static List<UpgradeValue> allValues = new List<UpgradeValue>();

    [SerializeField]
    private List<UpgradeData> upgrades;

    [SerializeField]
    private UpgradesScreen upgradesScreen;

    void Start()
    {
        Debug.Log("upgrade manager start");
        RegisterAll();
        upgradesScreen.SetUpgradeList(upgrades);
    }

    public static void RegisterUpgradeValue(UpgradeValue upgradeVal)
    {
        if (upgradeVal == null || upgradeVal.IsRegistered || string.IsNullOrEmpty(upgradeVal.BackendName))
            return;

        Debug.Log($"register value name: {upgradeVal.BackendName}");
        if (!allValues.Contains(upgradeVal))
            allValues.Add(upgradeVal);
        
        if (Instance == null)
            return;

        // Register this value with any upgrades that make use of it
        bool totalSuccess = false;
        for (int i = 0; i < Instance.upgrades.Count; ++i)
        {
            bool success = Instance.upgrades[i].TryRegisterValue(upgradeVal);
            if (success)
                totalSuccess = true;
        }
        if (!totalSuccess)
            Debug.LogError($"Could not find {upgradeVal.BackendName} in any upgrades");

        Instance.UpdateUpgradeValue(upgradeVal.BackendName);
        Debug.Log($"values count: {allValues.Count}");
    }

    public static void DeregisterUpgradeValue(UpgradeValue upgradeVal)
    {
        allValues.Remove(upgradeVal);
        for (int i = 0; i < Instance.upgrades.Count; ++i)
            Instance.upgrades[i].TryDeregisterValue(upgradeVal);
    }

    private void RegisterAll()
    {
        for(int i = 0; i < allValues.Count; ++i)
        {
            allValues[i].Register();
        }
    }

    public void UpdateAllUpgrades()
    {
        int i;
        for (i = 0; i < allValues.Count; ++i)
            allValues[i].ResetValue();

        for (i = 0; i < upgrades.Count; ++i)
            upgrades[i].UpdateAllValues();
    }

    public void UpdateUpgradeValue(string valueName)
    {
        for (int i = 0; i < upgrades.Count; ++i)
            upgrades[i].TryUpdateValue(valueName);
    }
}