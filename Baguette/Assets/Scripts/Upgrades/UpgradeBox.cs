using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Individual display and button for an upgrade level, to be used by UpgradeInfoDisplay
/// </summary>
public class UpgradeBox : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private Image itemImage;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private Button selectButton;

    [SerializeField]
    private Image outlineImage;

    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private Image innerImage;

    [SerializeField]
    private Color selectedColour;

    [SerializeField]
    private Color activeColour;

    [SerializeField]
    private Color defaultOutlineColour;

    [SerializeField]
    private Color defaultFillColour;

    private int upgradeLevel;

    private UpgradeData.LevelValue levelValue;

    private UpgradeInfoDisplay parentDisplay;

    private void Start()
    {
        if(selectButton)
            selectButton.onClick.AddListener(SelectUpgradeLevel);
    }

    private void SelectUpgradeLevel()
    {
        parentDisplay.StoreUpgradeLevel(upgradeLevel);
    }

    public void PopulateWithUpgrade(UpgradeData data, int selectLevel, UpgradeInfoDisplay display)
    {
        upgradeLevel = selectLevel;
        levelValue = data.GetLevelValue(selectLevel);

        PopulateWithParameters(levelValue.LevelName, levelValue.LevelSprite, levelValue.LevelCost);

        parentDisplay = display;
    }

    public void PopulateWithParameters(string name, Sprite boxSprite, string cost)
    {
        nameText.text = name;
        itemImage.sprite = boxSprite;
        costText.text = "€ " + cost;
    }

    public void SetSelected(bool inUse)
    {
        outlineImage.color = inUse ? selectedColour : defaultOutlineColour;
    }

    public void SetActive(bool inUse)
    {
        fillImage.color = inUse ? activeColour : defaultFillColour;
    }

    public void SetDisabled(bool notInUse)
    {
        fillImage.color = notInUse ? Color.grey : defaultFillColour;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!selectButton)
            return;

        TooltipHover.ShowTooltip(levelValue.Tooltip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selectButton)
            return;

        TooltipHover.HideTooltip();
    }
}