using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls for the main menu
/// </summary>
public class MainMenuScreen : MonoBehaviour
{
    [SerializeField]
    private Button playButton;

    [SerializeField]
    private Button practiceButton;

    [SerializeField]
    private Button settingsButton;

    [SerializeField]
    private Button quitButton;

    [SerializeField]
    private GameObject settingsScreen;

    void Start()
    {
        playButton.onClick.AddListener(PlayButtonClick);

        // Remove the dedicated practice button and make our normal
        // play button go to practice if tutorial hasn't been done
        if (PlayerPrefs.GetInt("tutorial_done", 0) == 0)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(PracticeButtonClick);
            Destroy(practiceButton.gameObject);
        }
        else
            practiceButton.onClick.AddListener(PracticeButtonClick);

        settingsButton.onClick.AddListener(SettingsButtonClick);
        quitButton.onClick.AddListener(QuitButtonClick);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PlayButtonClick()
    {
        FlowManager.Instance.LoadScene(Consts.PLAY_SCENE);
    }

    private void PracticeButtonClick()
    {
        TutorialController.InPracticeMode = true;
        PlayButtonClick();
    }

    private void SettingsButtonClick()
    {
        settingsScreen.SetActive(true);
    }

    private void QuitButtonClick()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}