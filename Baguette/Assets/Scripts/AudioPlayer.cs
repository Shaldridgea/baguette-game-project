using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player to manage multiple audio sources that are associated with a specific location in the game
/// </summary>
public class AudioPlayer : MonoBehaviour
{
    private static Dictionary<Consts.AudioLocation, AudioPlayer> audioPlayers = new Dictionary<Consts.AudioLocation, AudioPlayer>();

    [SerializeField]
    private Consts.AudioLocation location;

    [SerializeField]
    private AudioSource source;

    private List<AudioSource> sources = new List<AudioSource>();

    [SerializeField]
    private int sourcesStartAmount = 3;

    [SerializeField]
    private bool persistent;

    void Start()
    {
        // Only one audio player per location
        if (audioPlayers.ContainsKey(location))
        {
            Destroy(gameObject);
            return;
        }
        else
            audioPlayers.Add(location, this);

        if(persistent)
            DontDestroyOnLoad(gameObject);

        sources.Add(source);
        for (int i = 0; i < sourcesStartAmount; ++i)
            CreateNewSource();
    }

    private void OnDestroy()
    {
        if(!persistent)
            audioPlayers.Remove(location);
    }

    private void PlaySoundOneShot(AudioClip clip)
    {
        source.PlayOneShot(clip);
    }

    /// <summary>
    /// Play the supplied sound on the first free audio source, or allocate a new one if none free
    /// </summary>
    /// <param name="clip">Clip to play</param>
    /// <param name="loop">Whether to loop</param>
    /// <returns>The source the sound is being played on</returns>
    private AudioSource PlaySound(AudioClip clip, bool loop)
    {
        for(int i = 0; i < sources.Count; ++i)
        {
            if(!sources[i].isPlaying)
            {
                sources[i].clip = clip;
                sources[i].loop = loop;
                sources[i].Play();
                return sources[i];
            }
        }

        // If an audio source to play on was not already found, make a new one
        AudioSource newSource = CreateNewSource();
        newSource.clip = clip;
        newSource.loop = loop;
        newSource.Play();
        return newSource;
    }

    /// <summary>
    /// Play the supplied audio clip on the supplied audio source
    /// </summary>
    /// <param name="playSource">Audio source to play on</param>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="loop">Whether to loop</param>
    /// <returns>The source the clip is being played on</returns>
    private static AudioSource PlaySoundOnSource(AudioSource playSource, AudioClip clip, bool loop)
    {
        playSource.Stop();
        playSource.clip = clip;
        playSource.loop = loop;
        playSource.Play();
        return playSource;
    }

    /// <summary>
    /// Play the supplied audio clip at the desired audio location and loop it
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="loc">AudioLocation of where to play</param>
    /// <returns>The source the clip is being played on. Null if no audio player exists for location</returns>
    public static AudioSource PlayLooped(AudioClip clip, Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return null;

        return audioPlayers[loc].PlaySound(clip, true);
    }

    /// <summary>
    /// Play the supplied audio clip once at the desired location
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="loc">AudioLocation of where to play</param>
    /// <returns>The source the clip is being played on. Null if no audio player exists for location</returns>
    public static AudioSource PlayOnce(AudioClip clip, Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return null;

        return audioPlayers[loc].PlaySound(clip, false);
    }

    /// <summary>
    /// Play the supplied audio clip at the desired location without allocating it a dedicated audio source
    /// </summary>
    /// <param name="clip">Audio clip to play</param>
    /// <param name="loc">AudioLocation of where to play</param>
    /// <returns>The source the clip is being played on. Null if no audio player exists for location</returns>
    public static AudioSource PlayOnceFree(AudioClip clip, Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return null;

        audioPlayers[loc].PlaySoundOneShot(clip);
        return audioPlayers[loc].source;
    }

    /// <summary>
    /// Fade the old clip that's playing at the audio location out, before fading in the new clip to the same volume as before
    /// </summary>
    /// <param name="oldClip">Currently playing clip</param>
    /// <param name="newClip">New clip to play</param>
    /// <param name="loc">AudioLocation to fade/play clips</param>
    /// <param name="fadeOutTime">How many seconds to fade old clip to 0</param>
    /// <param name="fadeInTime">How many seconds to fade new clip to the previous volume</param>
    public static void FadeOldClipToNewClip(AudioClip oldClip, AudioClip newClip, Consts.AudioLocation loc, float fadeOutTime, float fadeInTime)
    {
        if (!audioPlayers.ContainsKey(loc))
            return;

        AudioSource sourceToFade = null;
        sourceToFade = GetSourceWithClip(oldClip, loc);

        if (sourceToFade == null)
            return;

        FadeSourceVolume(sourceToFade, 0f, fadeOutTime, () => PlaySoundOnSource(sourceToFade, newClip, false));
        FadeSourceVolume(sourceToFade, sourceToFade.volume, fadeInTime).setDelay(fadeOutTime);
    }

    /// <summary>
    /// Fade the supplied source to the new volume over time, with an optional callback on completion
    /// </summary>
    /// <param name="fadeSource">Audio source to fade</param>
    /// <param name="volumeGoal">New volume between 0.0 and 1.0</param>
    /// <param name="fadeTime">How long in seconds to fade for</param>
    /// <param name="fadeCompleteAction">Optional callback for the end of fading</param>
    /// <returns>Tween object for the volume fade. Null if given invalid source</returns>
    public static LTDescr FadeSourceVolume(AudioSource fadeSource, float volumeGoal, float fadeTime, System.Action fadeCompleteAction = null)
    {
        if (!fadeSource)
            return null;

        LTDescr volumeTween = LeanTween.value(fadeSource.gameObject, fadeSource.volume, volumeGoal, fadeTime).setOnUpdate((f) => fadeSource.volume = f);
        if (fadeCompleteAction != null)
            volumeTween.setOnComplete(fadeCompleteAction);

        return volumeTween;
    }

    /// <summary>
    /// Get the audio source with this clip attached at the audio location
    /// </summary>
    /// <param name="clip">Clip to check against</param>
    /// <param name="loc">AudioLocation to check in</param>
    /// <returns>The audio source this clip is attached to. Null if matching source wasn't found or no audio player exists for location</returns>
    public static AudioSource GetSourceWithClip(AudioClip clip, Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return null;

        AudioPlayer player = audioPlayers[loc];
        for (int i = 0; i < player.sources.Count; ++i)
        {
            if (player.sources[i].clip == clip)
            {
                return player.sources[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Stop all sounds at the audio location
    /// </summary>
    public static void StopAllSounds(Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return;

        AudioPlayer player = audioPlayers[loc];
        for (int i = 0; i < player.sources.Count; ++i)
            player.sources[i].Stop();
    }

    /// <summary>
    /// Stops the supplied clip playing sound
    /// </summary>
    /// <param name="clip">Clip to match against to stop</param>
    /// <param name="loc">AudioLocation for the source</param>
    public static void StopSound(AudioClip clip, Consts.AudioLocation loc = Consts.AudioLocation.Default)
    {
        if (!audioPlayers.ContainsKey(loc))
            return;

        AudioSource source = GetSourceWithClip(clip, loc);
        if (source != null)
            source.Stop();
    }

    /// <summary>
    /// Creates a new source on this player and adds to the list, copying its settings from the first source
    /// </summary>
    /// <returns>New audio source</returns>
    private AudioSource CreateNewSource()
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = source.playOnAwake;
        src.rolloffMode = source.rolloffMode;
        src.spatialBlend = source.spatialBlend;
        src.minDistance = source.minDistance;
        src.maxDistance = source.maxDistance;
        src.outputAudioMixerGroup = source.outputAudioMixerGroup;
        src.bypassEffects = source.bypassEffects;
        src.bypassListenerEffects = source.bypassListenerEffects;
        src.bypassReverbZones = source.bypassReverbZones;
        sources.Add(src);
        return src;
    }
}