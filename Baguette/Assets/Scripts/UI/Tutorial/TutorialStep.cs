using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data describing a step in the tutorial with what to say, what to highlight etc.
/// </summary>
[CreateAssetMenu(fileName = "New step", menuName = "Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [SerializeField]
    [TextArea]
    private string text;

    public string Text => text;

    [SerializeField]
    private Vector3 textPos;

    public Vector3 TextPosition => textPos;

    [SerializeField]
    private Vector2 textSize;

    public Vector2 TextSize => textSize;

    [SerializeField]
    private string trackingObjectName;

    public string TrackingObjectName => trackingObjectName;

    [SerializeField]
    private bool useSpotlight;

    public bool UseSpotlight => useSpotlight;

    [SerializeField]
    private string keyName;

    public string KeyName => keyName;

    [SerializeField]
    private string screenInput;

    public string ScreenInput => screenInput;
}