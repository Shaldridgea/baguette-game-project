using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the rotating of screens to face the camera
/// </summary>
public class CameraController : MonoBehaviour
{
    enum SCREEN
    {
        CENTRE,
        UP,
        LEFT,
        RIGHT
    }

    [SerializeField]
    private GameObject[] screens;

    [SerializeField]
    private string[] cameraInputs;

    [SerializeField]
    [Tooltip("Centre = 0, Up = 1, Left = 2, Right = 3")]
    private Vector3[] cameraRotations;

    [SerializeField]
    private float rotateTime;

    [SerializeField]
    [Range(0.01f, 1f)]
    private float quickRotateModifier;

    [SerializeField]
    [SearchableEnum]
    private LeanTweenType cameraEaseType;

    private SCREEN currentScreen;

    public static bool IsRotating => LeanTween.isTweening(tweenObject);

    public static Transform CurrentScreenTransform { get; private set; }

    private static GameObject tweenObject;

    private void Awake()
    {
        // Static reference for easy checking if the camera is rotating
        tweenObject = gameObject;
    }

    void Start()
    {
        FlowManager.Instance.StateStart += FocusCamera;
        InputManager.Instance.InputEvent += CheckForScreenInput;
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= FocusCamera;
        InputManager.Instance.InputEvent -= CheckForScreenInput;
    }

    public void FocusCamera(Consts.GameState newState)
    {
        if (newState == Consts.GameState.Results)
        {
            transform.localEulerAngles = cameraRotations[0];
            currentScreen = SCREEN.CENTRE;
            // Place our camera above the normal camera position
            // so we're not viewing the gameplay area
            Camera.main.transform.position = transform.position + new Vector3(0f, 22f);
        }
        else if (newState == Consts.GameState.Baking)
        {
            Camera.main.transform.position = transform.position;
        }
        CurrentScreenTransform = screens[(int)currentScreen].transform;
        AnimationFinish();
    }

    private void CheckForScreenInput(string inputID)
    {
        if (FlowManager.Instance.IsGamePaused)
            return;

        // This transform is the parent centre point for all
        // the screens which rotate around it. The camera is not
        // rotated; we rotate to move the screens in front of the camera
        for (int i = 0; i <= (int)SCREEN.RIGHT; ++i)
        {
            if (cameraInputs[i] == inputID)
            {
                SCREEN nextScreen = (SCREEN)i;
                if (currentScreen == nextScreen)
                    continue;
                
                ActivateScreens();
                Vector3 currentRot = transform.localEulerAngles;
                Vector3 nextRot = cameraRotations[i];
                // If x or y axis rotations are the same then we don't need
                // to rotate through the centre workspace to get to our target
                if (currentRot.x == nextRot.x || currentRot.y == nextRot.y)
                {
                    LeanTween.cancel(gameObject);
                    transform.LeanRotate(cameraRotations[i], rotateTime).setEase(cameraEaseType).setOnComplete(AnimationFinish);
                }
                else
                    ReturnToCentre(rotateTime * quickRotateModifier);
                currentScreen = nextScreen;
                CurrentScreenTransform = screens[(int)currentScreen].transform;
            }
        }
    }

    private void ReturnToCentre(float time)
    {
        LeanTween.cancel(gameObject);
        LTDescr tween = transform.LeanRotate(cameraRotations[(int)SCREEN.CENTRE], time).setEase(cameraEaseType).setOnComplete(ReturnCentreCallback);
    }

    private void ReturnCentreCallback()
    {
        // If we're not focusing on the centre screen then
        // we need to rotate to the actual screen we want
        if (currentScreen != SCREEN.CENTRE)
            transform.LeanRotate(cameraRotations[(int)currentScreen], rotateTime * quickRotateModifier).setEase(cameraEaseType).setOnComplete(AnimationFinish);
    }

    private void AnimationFinish()
    {
        // Keep only the screen we're looking at active
        if(!IsRotating)
            for (int i = 0; i < screens.Length; ++i)
                screens[i].SetActive(i == (int)currentScreen);
    }

    /// <summary>
    /// Activate all screens
    /// </summary>
    private void ActivateScreens()
    {
        for (int i = 0; i < screens.Length; ++i)
            screens[i].SetActive(true);
    }
}