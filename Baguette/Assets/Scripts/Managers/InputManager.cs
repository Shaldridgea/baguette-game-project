using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Input data holder and input handler
/// </summary>
public class InputManager : MonoBehaviour
{
    #region Singleton
    public static InputManager Instance { get; private set; }

    private InputManager() { }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    [Serializable]
    public class InputType
    {
        public InputType(InputType origin)
        {
            inputID = origin.inputID;
            keyCode = origin.keyCode;
            inputTrigger = origin.inputTrigger;
            isMouse = origin.isMouse;
            mouseButton = origin.mouseButton;
        }

        enum MOUSE_BUTTON
        {
            LEFT_MOUSE,
            RIGHT_MOUSE,
            MIDDLE_MOUSE
        }

        enum INPUT_TRIGGER
        {
            PRESS,
            HOLD,
            RELEASE
        }

        [SerializeField]
        private string inputID;
        [SerializeField]
        [KeyCode]
        private KeyCode keyCode;
        [SerializeField]
        private INPUT_TRIGGER inputTrigger;
        [SerializeField]
        private bool isMouse;
        [SerializeField]
        private MOUSE_BUTTON mouseButton;

        public bool IsOn { get; private set; }

        public string InputID => inputID;

        public int MouseButton => (int)mouseButton;

        public bool IsMouse => isMouse;

        public KeyCode Key => keyCode;

        public bool ReadInput()
        {
            bool inputOccurred = false;
            switch (inputTrigger)
            {
                case INPUT_TRIGGER.PRESS:
                    {
                        if (isMouse)
                            inputOccurred = Input.GetMouseButtonDown(MouseButton);
                        else
                            inputOccurred = Input.GetKeyDown(keyCode);
                        break;
                    }
                case INPUT_TRIGGER.HOLD:
                    {
                        if (isMouse)
                            inputOccurred = Input.GetMouseButton(MouseButton);
                        else
                            inputOccurred = Input.GetKey(keyCode);
                        break;
                    }
                case INPUT_TRIGGER.RELEASE:
                    {
                        if (isMouse)
                            inputOccurred = Input.GetMouseButtonUp(MouseButton);
                        else
                            inputOccurred = Input.GetKeyUp(keyCode);
                        break;
                    }
                default:
                    break;
            }
            IsOn = inputOccurred;
            return IsOn;
        }

        public string GetReadableInputString()
        {
            string s;
            if (isMouse)
                s = mouseButton.ToString();
            else
                s = keyCode.ToString();

            s = s.Replace('_', ' ');
            return s;
        }

        public void RebindInput(bool isMouse, int inputCode)
        {
            this.isMouse = isMouse;
            if (isMouse)
                mouseButton = (MOUSE_BUTTON)inputCode;
            else
                keyCode = (KeyCode)inputCode;
        }
    }

    [SerializeField]
    private List<InputType> inputList;

    private Dictionary<string, InputType> inputDict;

    private Dictionary<string, InputType> defaultInputDict;

    public delegate void InputDelegate(string inputID);

    public event InputDelegate InputEvent;

    public Vector3 MouseDelta { get; private set; }

    void Start()
    {
        inputDict = new Dictionary<string, InputType>();
        defaultInputDict = new Dictionary<string, InputType>();
        foreach (InputType input in inputList)
        {
            inputDict.Add(input.InputID, input);
            defaultInputDict.Add(input.InputID, new InputType(input));
        }
    }

    void Update()
    {
        for (int i = 0; i < inputList.Count; ++i)
            inputList[i].ReadInput();

        for (int i = 0; i < inputList.Count; ++i)
        {
            if(inputList[i].IsOn)
                InputEvent?.Invoke(inputList[i].InputID);
        }

        MouseDelta = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    public bool GetInput(string id)
    {
        if(inputDict.TryGetValue(id, out InputType input))
            return input.IsOn;

        return false;
    }

    public InputType GetInputType(string id)
    {
        if (inputDict.TryGetValue(id, out InputType input))
            return input;
        else
            return null;
    }

    public InputType GetInputDefault(string id)
    {
        if (defaultInputDict.TryGetValue(id, out InputType input))
            return input;
        else
            return null;
    }

    public InputType RebindInputType(string inputToChange, bool isMouse, int inputCode)
    {
        InputType input = inputDict[inputToChange];
        input.RebindInput(isMouse, inputCode);
        return input;
    }
}