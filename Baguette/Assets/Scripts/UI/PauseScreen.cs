using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause screen display and control
/// </summary>
public class PauseScreen : MonoBehaviour
{
    [SerializeField]
    private Button resumeButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private Canvas uiCanvas;

    [SerializeField]
    private SettingsScreen settingsPrefab;

    private SettingsScreen settingsScreen;

    // Start is called before the first frame update
    void Start()
    {
        resumeButton.onClick.AddListener(ResumeButtonClicked);
        settingsButton.onClick.AddListener(SettingsButtonClicked);
        quitButton.onClick.AddListener(QuitButtonClicked);

        FlowManager.Instance.PauseEvent += PauseEventRaised;
        FlowManager.Instance.UnpauseEvent += UnpauseEventRaised;
        InputManager.Instance.InputEvent += CheckForPauseInput;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        FlowManager.Instance.PauseEvent -= PauseEventRaised;
        FlowManager.Instance.UnpauseEvent -= UnpauseEventRaised;
        InputManager.Instance.InputEvent -= CheckForPauseInput;
    }

    private void CheckForPauseInput(string inputID)
    {
        if (inputID != "Pause")
            return;

        if (!FlowManager.Instance.IsGamePaused)
            FlowManager.Instance.Pause();
        else
            FlowManager.Instance.Unpause();
    }

    private void PauseEventRaised(Consts.GameState state)
    {
        gameObject.SetActive(true);
    }

    private void UnpauseEventRaised(Consts.GameState state)
    {
        if(settingsScreen != null)
            settingsScreen.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void ResumeButtonClicked()
    {
        FlowManager.Instance.Unpause();
    }

    private void SettingsButtonClicked()
    {
        if (settingsScreen == null)
        {
            settingsScreen = Instantiate(settingsPrefab, uiCanvas.transform);
            settingsScreen.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
        }
        else
            settingsScreen.gameObject.SetActive(true);
    }

    private void QuitButtonClicked()
    {
        resumeButton.onClick.RemoveAllListeners();
        settingsButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

        FlowManager.Instance.PauseEvent -= PauseEventRaised;
        FlowManager.Instance.UnpauseEvent -= UnpauseEventRaised;
        InputManager.Instance.InputEvent -= CheckForPauseInput;

        FlowManager.Instance.Unpause();
        FlowManager.Instance.LoadScene(Consts.MAIN_MENU_SCENE);
    }
}
