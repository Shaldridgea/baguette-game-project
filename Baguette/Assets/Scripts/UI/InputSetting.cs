using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI button element for rebinding an input
/// </summary>
public class InputSetting : MonoBehaviour
{
    [SerializeField]
    private string inputName;

    [SerializeField]
    private string[] inputsToChange;

    [SerializeField]
    private TextMeshProUGUI displayText;

    [SerializeField]
    private TextMeshProUGUI buttonText;

    [SerializeField]
    private GranularButton changeButton;

    private bool waitingForInput;

    private int delayWaitForInput;

    private static InputSetting waitingSetting;

    void Start()
    {
        changeButton.onLeftClick.AddListener(ChangeButtonClicked);
        changeButton.onRightClick.AddListener(ResetToDefault);
        displayText.text = inputName;
        buttonText.text = InputManager.Instance.GetInputType(inputsToChange[0]).GetReadableInputString();
    }

    private void Update()
    {
        if (!waitingForInput)
            return;

        if (delayWaitForInput > 0)
        {
            --delayWaitForInput;
            return;
        }

        for(int i = 0; i <= 2; ++i)
            if(Input.GetMouseButtonUp(i))
            {
                ChangeSetting(true, i);
                return;
            }

        if (Input.anyKeyDown)
        {
            for (KeyCode k = KeyCode.Backspace; k <= KeyCode.AltGr; ++k)
                if (Input.GetKeyDown(k))
                {
                    ChangeSetting(false, (int)k);
                    return;
                }
        }
    }

    private void ChangeButtonClicked()
    {
        if (waitingSetting != null)
            return;

        changeButton.interactable = false;
        buttonText.text = "Waiting...";
        waitingForInput = true;
        // Wait one frame before rebinding input
        delayWaitForInput = 1;
        waitingSetting = this;
    }

    private void ChangeSetting(bool isMouse, int inputCode)
    {
        waitingForInput = false;
        waitingSetting = null;

        InputManager.InputType input = null;
        for(int i = 0; i < inputsToChange.Length; ++i)
            input = InputManager.Instance.RebindInputType(inputsToChange[i], isMouse, inputCode);
        
        buttonText.text = input.GetReadableInputString();
        changeButton.interactable = true;
    }

    private void ResetToDefault()
    {
        for (int i = 0; i < inputsToChange.Length; ++i)
        {
            InputManager.InputType defaultInput = InputManager.Instance.GetInputDefault(inputsToChange[i]);
            if (defaultInput.IsMouse)
                ChangeSetting(true, defaultInput.MouseButton);
            else
                ChangeSetting(false, (int)defaultInput.Key);
        }
    }

    private void OnDisable()
    {
        waitingSetting = null;
    }

    private void OnValidate()
    {
        if (displayText == null)
            return;
        
        displayText.text = inputName;
    }
}