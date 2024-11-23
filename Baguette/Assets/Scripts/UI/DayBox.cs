using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Day box display used in the calendar screen
/// </summary>
public class DayBox : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI numberText;

    [SerializeField]
    private GameObject circleObject;

    public void SetDayNumber(int number)
    {
        numberText.text = number.ToString();
    }

    public void SetCircleVisible(bool isVisible)
    {
        circleObject.SetActive(isVisible);
    }
}