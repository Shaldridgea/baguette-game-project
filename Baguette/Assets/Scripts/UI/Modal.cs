using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controller for the screen fade modal
/// </summary>
public class Modal : MonoBehaviour
{
    [SerializeField]
    private Image modalImage;

    [SerializeField]
    private GameObject dontDestroyObject;

    private static Modal instance;

    private static Color currentColour;

    private void Awake()
    {
        if (instance != null)
            Destroy(instance.dontDestroyObject);

        instance = this;
        UpdateAlpha(currentColour.a);
        DontDestroyOnLoad(dontDestroyObject);
    }
    
    public static LTDescr Fade(float alpha, float time)
    {
        instance.modalImage.raycastTarget = true;
        return LeanTween.value(instance.gameObject, instance.modalImage.color.a, alpha, time).setOnUpdate(UpdateAlpha);
    }

    private static void UpdateAlpha(float a)
    {
        currentColour.a = a;
        instance.modalImage.color = currentColour;
        instance.modalImage.raycastTarget = currentColour.a != 0f;
    }
}