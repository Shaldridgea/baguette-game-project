using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls playing the assigned music for different game states and fading them
/// </summary>
public class MusicController : MonoBehaviour
{
    [SerializeField]
    private AudioClip calmMusic;

    [SerializeField]
    private AudioClip busyMusic;

    void Start()
    {
        FlowManager.Instance.StateStart += CheckForStateStart;
        FlowManager.Instance.StateEnd += CheckForStateEnd;
        AudioPlayer.PlayLooped(calmMusic, Consts.AudioLocation.Music);
    }

    private void OnDestroy()
    {
        FlowManager.Instance.StateStart -= CheckForStateStart;
        FlowManager.Instance.StateEnd -= CheckForStateEnd;
    }

    private void CheckForStateStart(Consts.GameState newState)
    {
        // Fade in playing the desired music for this section
        switch (newState)
        {
            case Consts.GameState.MainMenu:
                LeanTween.cancel(gameObject);
                AudioPlayer.FadeSourceVolume(AudioPlayer.PlayLooped(calmMusic, Consts.AudioLocation.Music), 1f, FlowManager.Instance.FadeTime * 2f);
            break;

            case Consts.GameState.Baking:
                LeanTween.cancel(gameObject);
                AudioPlayer.FadeSourceVolume(AudioPlayer.PlayLooped(busyMusic, Consts.AudioLocation.Music), 1f, FlowManager.Instance.FadeTime * 2f);
            break;

            default:
            break;
        }
    }

    private void CheckForStateEnd(Consts.GameState oldState)
    {
        // Fade out the currently playing music and stop the source playing when it ends
        switch (oldState)
        {
            case Consts.GameState.MainMenu:
                LeanTween.cancel(gameObject);
                AudioPlayer.FadeSourceVolume(AudioPlayer.GetSourceWithClip(calmMusic, Consts.AudioLocation.Music), 0f,
                    FlowManager.Instance.FadeTime * 0.5f, () => AudioPlayer.StopSound(calmMusic, Consts.AudioLocation.Music));
            break;

            case Consts.GameState.Baking:
            case Consts.GameState.Results:
            case Consts.GameState.Upgrades:
            case Consts.GameState.Calendar:
            {
                LeanTween.cancel(gameObject);
                AudioSource source = AudioPlayer.GetSourceWithClip(busyMusic, Consts.AudioLocation.Music);
                if (source != null && source.isPlaying)
                {
                    AudioPlayer.FadeSourceVolume(source, 0f, FlowManager.Instance.FadeTime * 0.5f,
                        () => AudioPlayer.StopSound(busyMusic, Consts.AudioLocation.Music));
                }
                break;
            }

            default:
            break;
        }
    }
}