using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Specialised button with events for left click and right click specifically
/// </summary>
public class GranularButton : Button, IPointerDownHandler
{
    public ButtonClickedEvent onLeftClick { get; set; }

    public ButtonClickedEvent onRightClick { get; set; }

    protected override void Awake()
    {
        base.Awake();
        onLeftClick = new ButtonClickedEvent();
        onRightClick = new ButtonClickedEvent();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!interactable)
            return;

        DoStateTransition(SelectionState.Normal, false);
        onClick?.Invoke();
        if (eventData.button == PointerEventData.InputButton.Left)
            onLeftClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            onRightClick?.Invoke();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable)
            return;

        DoStateTransition(SelectionState.Pressed, false);
    }
}