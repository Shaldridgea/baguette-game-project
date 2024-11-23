using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the flow between scenes and different game states
/// </summary>
public class FlowManager : MonoBehaviour
{
    #region Singleton
    public static FlowManager Instance { get; private set; }

    private FlowManager() { }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
            Destroy(gameObject);
    }
    #endregion

    [SerializeField]
    private float fadeDelay;

    [SerializeField]
    private float fadeTime;

    public float FadeDelay => fadeDelay;

    public float FadeTime => fadeTime;

    public bool IsGamePaused { get; private set; }

    public delegate void FlowChangeDelegate(Consts.GameState state);

    public event FlowChangeDelegate StateStart;

    public event FlowChangeDelegate StateEnd;

    public event FlowChangeDelegate PauseEvent;

    public event FlowChangeDelegate UnpauseEvent;

    private LinkedList<Consts.GameState> states = new LinkedList<Consts.GameState>();

    private LinkedListNode<Consts.GameState> currentState;

    private bool isInTargetScene;

    private string targetSceneName;

    private LTDescr stateFade;

    void Start()
    {
        // These are floating static data classes that don't need to be assigned
        new PlayerStats();
        new DayStats();

        for(Consts.GameState i = 0; i < Consts.GameState.Last; ++i)
            states.AddLast(i);

        currentState = states.First;

        StateStart += CheckForFirstState;
        StateEnd += FirstNodeRemoval;
        SceneManager.activeSceneChanged += ActiveSceneChange;
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            Progress();
    }
#endif

    public void Progress()
    {
        StartCoroutine(ProgressCoroutine());
    }

    private IEnumerator ProgressCoroutine()
    {
        StateEnd?.Invoke(currentState.Value);

        // Make a modal fade in and out to hide game state changes
        stateFade = Modal.Fade(1f, fadeTime);

        if (fadeTime > 0f)
            yield return new WaitForSecondsRealtime(fadeTime);

        if (fadeDelay > 0f)
            yield return new WaitForSecondsRealtime(fadeDelay);

        if (!isInTargetScene)
            yield return new WaitUntil(IsNextSceneActive);

        if (currentState.Next == null)
            currentState = states.First;
        else
            currentState = currentState.Next;

        stateFade = null;
        StateStart?.Invoke(currentState.Value);
        Modal.Fade(0f, fadeTime);
        yield break;
    }

    public void LoadScene(string sceneName)
    {
        targetSceneName = sceneName;
        if (targetSceneName == Consts.MAIN_MENU_SCENE)
        {
            // When we re-enter the main menu, re-create our state list
            // and set state to the last one, so that when we make
            // the Progress call it loops back to the main menu
            states.Clear();
            for (Consts.GameState i = 0; i < Consts.GameState.Last; ++i)
                states.AddLast(i);
            currentState = states.Last;
        }
        else if (targetSceneName == Consts.PLAY_SCENE)
        {
            // When in practice mode we don't care about
            // calendar and upgrades screens
            if(TutorialController.InPracticeMode)
            {
                states.Remove(Consts.GameState.Calendar);
                states.Remove(Consts.GameState.Upgrades);
            }
        }
        // Modal fade is started here
        Progress();

        // Callback to actually load the targeted scene
        // when our first modal fade finishes
        stateFade?.setOnComplete(LoadNextScene).setIgnoreTimeScale(true);
    }

    private void LoadNextScene()
    {
        SceneManager.LoadSceneAsync(targetSceneName);
    }

    private void ActiveSceneChange(Scene current, Scene next)
    {
        CheckForNextActiveScene(next);
    }

    private void CheckForNextActiveScene(Scene active)
    {
        isInTargetScene = active.name == targetSceneName;
    }

    private bool IsNextSceneActive() => isInTargetScene;

    private void CheckForFirstState(Consts.GameState newState)
    {
        if (newState != Consts.GameState.MainMenu)
            return;

        StateEnd += FirstNodeRemoval;
        Modal.Fade(0f, fadeTime);
    }

    private void FirstNodeRemoval(Consts.GameState oldState)
    {
        // We remove our first game state, the menu screen,
        // when we leave the main menu because it's not part
        // of the main gameplay loop
        if (oldState == Consts.GameState.MainMenu)
        {
            states.RemoveFirst();
            StateEnd -= FirstNodeRemoval;
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        IsGamePaused = true;
        PauseEvent?.Invoke(currentState.Value);
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
        IsGamePaused = false;
        UnpauseEvent?.Invoke(currentState.Value);
    }
}