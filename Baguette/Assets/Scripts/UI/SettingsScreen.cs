using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Settings UI screen and applying changed settings
/// </summary>
public class SettingsScreen : MonoBehaviour
{
    [SerializeField]
    private AudioMixer volumeMixer;

    [SerializeField]
    private Slider masterVolumeSlider;

    [SerializeField]
    private Slider sfxVolumeSlider;

    [SerializeField]
    private Slider musicVolumeSlider;

    [SerializeField]
    [Range(-80f, 0f)]
    private float muteVolumeLevel;

    [SerializeField]
    private Button backButton;

    private Dictionary<Slider, float> sliderValueDict;

    private Dictionary<Slider, string> sliderMixerDict;

    // Start is called before the first frame update
    void Start()
    {
        sliderMixerDict = new Dictionary<Slider, string>
        {
            { masterVolumeSlider, "MasterVolume" },
            { sfxVolumeSlider, "SFXVolume" },
            { musicVolumeSlider, "MusicVolume" }
        };

        SetSliderValueFromVolume(masterVolumeSlider);
        SetSliderValueFromVolume(sfxVolumeSlider);
        SetSliderValueFromVolume(musicVolumeSlider);

        sliderValueDict = new Dictionary<Slider, float>
        {
            { masterVolumeSlider, masterVolumeSlider.value },
            { sfxVolumeSlider, sfxVolumeSlider.value },
            { musicVolumeSlider, musicVolumeSlider.value }
        };

        masterVolumeSlider.onValueChanged.AddListener(SliderChangedEvent);
        sfxVolumeSlider.onValueChanged.AddListener(SliderChangedEvent);
        musicVolumeSlider.onValueChanged.AddListener(SliderChangedEvent);
        backButton.onClick.AddListener(BackButtonClicked);
    }

    private void SliderChangedEvent(float value)
    {
        Slider changedSlider = null;
        // Can find the slider that's been changed by checking
        // actual value against cached value in the dictionary
        foreach(KeyValuePair<Slider, float> i in sliderValueDict)
            if (i.Key.value != i.Value)
            {
                changedSlider = i.Key;
                break;
            }

        SetVolume(sliderMixerDict[changedSlider], muteVolumeLevel + value * Mathf.Abs(muteVolumeLevel));

        sliderValueDict[changedSlider] = value;
    }

    private void SetVolume(string volumeParameter, float newVolume)
    {
        volumeMixer.SetFloat(volumeParameter, newVolume);
    }

    private void SetSliderValueFromVolume(Slider volumeSlider)
    {
        volumeMixer.GetFloat(sliderMixerDict[volumeSlider], out float vol);
        volumeSlider.value = 1f - (vol / muteVolumeLevel);
    }

    private void BackButtonClicked()
    {
        gameObject.SetActive(false);
    }
}