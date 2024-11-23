using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Controls the flow and display of tutorial items, graphics, and practice mode
/// </summary>
public class TutorialController : MonoBehaviour
{
    [SerializeField]
    private TextMeshPro tutorialText;

    [SerializeField]
    private SpriteRenderer darkness;

    [SerializeField]
    private Transform spotlight;

    [SerializeField]
    private GameObject baguetteGuide;

    [SerializeField]
    private SpriteRenderer[] keyPrompts;

    [SerializeField]
    private Sprite keySprite;

    [SerializeField]
    private Sprite[] mouseSprites;

    [SerializeField]
    private int bakingSectionThreshold;

    [SerializeField]
    private List<TutorialStep> steps;

    private int stepIndex;

    private TextMeshPro[] keyPromptTexts;

    private TutorialStep currentStep;

    private Transform trackingObject;

    private bool findTrackingObjectFlag;

    private bool shouldShowGuide;

    public static bool InPracticeMode;

    void Start()
    {
        // We shouldn't exist outside of practice mode
        if (!InPracticeMode)
        {
            Destroy(gameObject);
            Destroy(baguetteGuide);
            return;
        }

        // Get text objects attached to our key prompt objects
        keyPromptTexts = new TextMeshPro[keyPrompts.Length];
        for (int i = 0; i < keyPromptTexts.Length; ++i)
            keyPromptTexts[i] = keyPrompts[i].GetComponentInChildren<TextMeshPro>();

        InputManager.Instance.InputEvent += TutorialInputs;
        
        StartStep(stepIndex);
        DayTimeManager.Instance.HourEvent += HourResetTime;

        // Baguette guide should start turned off until
        // needed unless we've seen tutorial already
        if (PlayerPrefs.GetInt("tutorial_done", 0) == 0)
        {
            baguetteGuide.SetActive(false);
            PlayerPrefs.SetInt("tutorial_done", 1);
        }
        else
            baguetteGuide.SetActive(true);
    }

    private void HourResetTime(float timeInHours)
    {
        // We reset our day at the end of the first
        // hour so practice mode is infinite
        DayTimeManager.Instance.SetTime(0, 0);
    }

    private void OnDestroy()
    {
        InputManager.Instance.InputEvent -= TutorialInputs;
        InPracticeMode = false;
    }

    private void StartStep(int newStep)
    {
        if (newStep < 0 || newStep >= steps.Count)
            return;

        // Don't show our baguette guide until we reach
        // the part of the tutorial where it's needed
        shouldShowGuide = newStep >= bakingSectionThreshold;
        if(shouldShowGuide)
            baguetteGuide.SetActive(true);

        currentStep = steps[newStep];
        tutorialText.text = ParseText(currentStep.Text);
        tutorialText.transform.position = currentStep.TextPosition;

        if (currentStep.TextSize.x > 0f)
            tutorialText.rectTransform.sizeDelta = new Vector2(currentStep.TextSize.x, tutorialText.rectTransform.sizeDelta.y);
        if (currentStep.TextSize.y > 0f)
            tutorialText.rectTransform.sizeDelta = new Vector2(tutorialText.rectTransform.sizeDelta.x, currentStep.TextSize.y);
        
        spotlight.gameObject.SetActive(currentStep.UseSpotlight);

        FindTrackingObject();

        stepIndex = newStep;
    }

    /// <summary>
    /// Find any object that may be tagged in the tutorial step info to be spotlighted
    /// </summary>
    private void FindTrackingObject()
    {
        // Find the object by name or tag in the scene
        if (!string.IsNullOrEmpty(currentStep.TrackingObjectName))
        {
            GameObject found;
            if (currentStep.TrackingObjectName.StartsWith("Tag="))
                found = GameObject.FindWithTag(currentStep.TrackingObjectName.Replace("Tag=", string.Empty));
            else
                found = GameObject.Find(currentStep.TrackingObjectName);
            if (found != null)
                trackingObject = found.transform;
            else
                trackingObject = null;
        }
        else
            trackingObject = null;
        
        // Here we can spotlight the use of certain keys for moving around the game screens
        for (int i = 0; i < keyPrompts.Length; ++i)
        {
            if (trackingObject == null && currentStep.UseSpotlight && keyPrompts[i].name == currentStep.KeyName)
            {
                trackingObject = keyPrompts[i].transform;
                // Change our input prompt sprite if necessary to reflect current key bindings
                var type = InputManager.Instance.GetInputType(currentStep.ScreenInput);
                if (type.IsMouse)
                {
                    keyPrompts[i].sprite = mouseSprites[type.MouseButton];
                    keyPromptTexts[i].text = string.Empty;
                }
                else
                {
                    keyPrompts[i].sprite = keySprite;
                    keyPromptTexts[i].text = type.GetReadableInputString().ToUpper();
                }
            }
            else
                keyPrompts[i].gameObject.SetActive(false);
        }

        if(trackingObject != null)
            trackingObject.gameObject.SetActive(true);
    }

    private string ParseText(string newText)
    {
        if (!newText.Contains("{"))
            return newText;

        List<int> firstIndexes = new List<int>();
        List<int> secondIndexes = new List<int>();

        // Find indexes of bracket starts and ends.
        // Doesn't support nested brackets
        for(int i = 0; i < newText.Length; ++i)
        {
            if (newText[i] == '{')
                firstIndexes.Add(i);
            else if (newText[i] == '}')
                secondIndexes.Add(i);
        }

        if(firstIndexes.Count != secondIndexes.Count)
        {
            Debug.Log("BRACKETS ARE MISSING IN TUTORIAL TEXT");
            return newText;
        }
        
        for (int i = firstIndexes.Count-1; i >= 0; --i)
        {
            int subLength = secondIndexes[i] - firstIndexes[i];
            // Get text between brackets
            string inputID = newText.Substring(firstIndexes[i] + 1, subLength - 1);
            // Replace original text and brackets with corresponding input name
            newText = newText.Replace(newText.Substring(firstIndexes[i], subLength + 1),
                InputManager.Instance.GetInputType(inputID).GetReadableInputString());
        }
        return newText;
    }

    private void Update()
    {
        if (trackingObject == null)
            return;

        // Re-find tracking objects when changing screens
        if (CameraController.IsRotating)
            findTrackingObjectFlag = true;
        else if(findTrackingObjectFlag)
        {
            findTrackingObjectFlag = false;
            FindTrackingObject();
        }

        spotlight.position = trackingObject.position;
    }

    private void TutorialInputs(string inputID)
    {
        if (inputID == "ToggleTutorial")
        {
            gameObject.SetActive(!gameObject.activeSelf);
            
            if (gameObject.activeSelf)
                StartStep(stepIndex);
        }

        else if (inputID == "NextTutorial")
            StartStep(stepIndex + 1);

        else if (inputID == "PreviousTutorial")
            StartStep(stepIndex - 1);
    }
}