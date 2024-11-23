using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls randomising graphics that use a fill and outline, and editing their display
/// </summary>
public class PanelGraphics : MonoBehaviour
{
    [SerializeField]
    private PanelData panelData;

    [SerializeField]
    private bool isLarge;

    [SerializeField]
    private bool backgroundFill;

    [SerializeField]
    private Color fillColour = Color.white;

    [SerializeField]
    private bool changeColour = true;

    private Image backgroundImg;

    private Image fillImg;

    private Image outlineImg;

    private static PanelData dataInstance;

    private void OnEnable()
    {
        if (!Application.isPlaying)
            return;

        FindImages();
        ApplyChanges();
    }

    private void OnValidate()
    {
        FindImages();
        ApplyChanges();
    }

    /// <summary>
    /// Finds the UI image components on this object to be edited
    /// </summary>
    private void FindImages()
    {
        if (panelData && dataInstance == null)
            dataInstance = panelData;

        if (!backgroundImg)
            backgroundImg = GetComponent<Image>();

        if (!fillImg || !outlineImg)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                var child = transform.GetChild(i);
                if (child.name.ToLower().Contains("fill"))
                    fillImg = transform.GetComponent<Image>();

                if(child.name.ToLower().Contains("outline"))
                    outlineImg = child.GetComponent<Image>();
            }
        }
    }

    /// <summary>
    /// Apply edited changes to graphics
    /// </summary>
    private void ApplyChanges()
    {
        if (!panelData)
            panelData = dataInstance;

        if (!panelData)
            return;

        Sprite[] graphics = panelData.GetRandomGraphics(isLarge, !Application.isPlaying);

        if(backgroundImg)
        {
            backgroundImg.sprite = graphics[0];
            backgroundImg.enabled = backgroundFill;
        }

        if(fillImg)
        {
            fillImg.sprite = graphics[0];
            if(changeColour)
                fillImg.color = fillColour;
        }

        if(outlineImg)
        {
            outlineImg.sprite = graphics[1];
        }
    }
}