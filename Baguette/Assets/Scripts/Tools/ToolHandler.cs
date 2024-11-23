using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

/// <summary>
/// Spawns and cleans up in-game tools
/// </summary>
public class ToolHandler : MonoBehaviour
{
    [System.Serializable]
    public class StringPrefabDictionary : SerializableDictionaryBase<string, PlayerTool> { }

    [System.Serializable]
    public class ToolSpawnInfo
    {
        [SerializeField]
        private UpgradeValueString toolStringID;

        public UpgradeValueString UpgradeValue => toolStringID;

        public string ToolID => toolStringID;

        [SerializeField]
        private Vector3 spawnPosition;

        public Vector3 SpawnPos => spawnPosition;

        [SerializeField]
        private Transform spawnScreen;

        public Transform SpawnScreen => spawnScreen;
    }

    [SerializeField]
    private StringPrefabDictionary toolPrefabDict;

    [SerializeField]
    private List<ToolSpawnInfo> toolSpawnList;

    private List<PlayerTool> currentTools = new List<PlayerTool>();

    private bool cleanupRequired;

    private void Start()
    {
        PlayerTool.SetHandler(this);
        for (int i = 0; i < toolSpawnList.Count; ++i)
            toolSpawnList[i].UpgradeValue.Init();
        FlowManager.Instance.StateStart += CheckForBakeStart;
        FlowManager.Instance.StateEnd += CheckForBakeEnd;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForBakeStart;
        FlowManager.Instance.StateEnd -= CheckForBakeEnd;
        for (int i = 0; i < toolSpawnList.Count; ++i)
            toolSpawnList[i].UpgradeValue.Deregister();
    }

    private void CheckForBakeStart(Consts.GameState newState)
    {
        // Clean up when we start a new state when
        // the baking screen isn't visible
        if (newState != Consts.GameState.Baking)
        {
            if (cleanupRequired)
            {
                for (int i = 0; i < currentTools.Count; ++i)
                    Destroy(currentTools[i].gameObject);

                currentTools.Clear();
            }
            return;
        }

        for (int i = 0; i < toolSpawnList.Count; ++i)
            SpawnTool(toolSpawnList[i]);
    }

    private void CheckForBakeEnd(Consts.GameState oldState)
    {
        if (oldState != Consts.GameState.Baking)
        {
            cleanupRequired = false;
            return;
        }

        cleanupRequired = true;
    }

    /// <summary>
    /// Spawn a tool in the gameplay area according to its spawn information
    /// </summary>
    /// <param name="spawnInfo">Tool to spawn</param>
    public void SpawnTool(ToolSpawnInfo spawnInfo)
    {
        if (string.IsNullOrEmpty(spawnInfo.ToolID))
            return;

        if (!toolPrefabDict.ContainsKey(spawnInfo.ToolID))
            return;

        PlayerTool toolPrefab = toolPrefabDict[spawnInfo.ToolID];

        PlayerTool newTool = Instantiate
            (toolPrefab, Vector3.zero, spawnInfo.SpawnScreen.rotation * toolPrefab.transform.localRotation,
            spawnInfo.SpawnScreen);

        newTool.transform.localPosition = spawnInfo.SpawnPos;
        newTool.SetInfo(spawnInfo);
        currentTools.Add(newTool);
    }

    public void ReplaceTool(ToolSpawnInfo spawnInfo)
    {
        if (cleanupRequired)
            return;

        SpawnTool(spawnInfo);
    }

    public void RemoveTool(PlayerTool toolObject) => currentTools.Remove(toolObject);
}