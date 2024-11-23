using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility class used by ToolHandler on tool objects to control their spawning
/// </summary>
public class PlayerTool : MonoBehaviour
{
    private static ToolHandler handler;

    private ToolHandler.ToolSpawnInfo spawnInfo;

    public static void SetHandler(ToolHandler newHandler) => handler = newHandler;

    public void SetInfo(ToolHandler.ToolSpawnInfo newInfo) => spawnInfo = newInfo;
    
    public void Replace() => handler.ReplaceTool(spawnInfo);

    private void OnDestroy() => handler.RemoveTool(this); 
}