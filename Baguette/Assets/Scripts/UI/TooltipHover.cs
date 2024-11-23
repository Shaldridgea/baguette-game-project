using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles showing a UI tooltip at the mouse position when called
/// </summary>
public class TooltipHover : MonoBehaviour
{
    private static TooltipHover tooltipInstance;

    [SerializeField]
    private TextMeshProUGUI tooltipText;

    [SerializeField]
    [Range(10f, 50f)]
    [Tooltip("When resizing the tooltip box to the text's length, what's the max length?")]
    private float textLengthMaxModifier = 50f;

    [SerializeField]
    private float minScaleSize = 0.85f;

    [SerializeField]
    private float maxScaleSize = 1.5f;

    private RectTransform thisRect;

    private Vector3 size;

    private void Awake()
    {
        tooltipInstance = this;
        thisRect = (RectTransform)transform;
        size = thisRect.sizeDelta;
        gameObject.SetActive(false);
    }

    public static void ShowTooltip(string newText)
    {
        tooltipInstance.tooltipText.text = newText;
        tooltipInstance.gameObject.SetActive(true);
        tooltipInstance.transform.position = Input.mousePosition;
        tooltipInstance.thisRect.sizeDelta = tooltipInstance.size *
            Mathf.Clamp(newText.Length / tooltipInstance.textLengthMaxModifier, tooltipInstance.minScaleSize, tooltipInstance.maxScaleSize);
    }

    public static void HideTooltip()
    {
        tooltipInstance.gameObject.SetActive(false);
    }

    void LateUpdate()
    {
        transform.position = Input.mousePosition;
    }
}