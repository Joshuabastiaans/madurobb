using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum Emotion
    {
        Neutral,
        Scared,
        Positive
    }

    [Header("Ambient Audio")]
    public AudioClip engineLoop;
    public AudioClip waterLoop;
    public AudioClip seagullsLoop;

    [Header("Crowd Audio")]
    public List<AudioClip> neutralCrowdClips;
    public List<AudioClip> scaredCrowdClips;
    public List<AudioClip> positiveCrowdClips;

    [Header("Urgency Audio")]
    public AudioClip alarmLoop;
    public AudioClip fireLoop;

    [Header("Voice Lines")]
    public AudioClip inactivityClip;

    [Header("Audio Sources")]
    public AudioSource ambientSourceEngine;
    public AudioSource ambientSourceWater;
    public AudioSource ambientSourceSeagulls;

    public AudioSource crowdSource;     // Will switch between emotions
    public AudioSource crowdOneShotSource; // For occasional neutral lines

    public AudioSource urgencySourceAlarm;
    public AudioSource urgencySourceFire;

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float ambientVolume = 0.8f;
    [Range(0f, 1f)] public float crowdVolume = 1.0f;
    [Range(0f, 1f)] public float urgencyVolume = 1.0f;

    private Emotion currentEmotion = Emotion.Neutral;
    private bool waveActive = false;

    void Start()
    {
        // Initialize loops
        SetupLoop(ambientSourceEngine, engineLoop, ambientVolume);
        SetupLoop(ambientSourceWater, waterLoop, ambientVolume);
        SetupLoop(ambientSourceSeagulls, seagullsLoop, ambientVolume);

        // Initially, no urgency
        SetupLoop(urgencySourceAlarm, alarmLoop, 0f); // start off muted
        SetupLoop(urgencySourceFire, fireLoop, 0f);   // start off muted

        // Start with crowd as neutral or silent. We can fade in later.
        crowdSource.loop = true;
        crowdSource.volume = 0f;
        currentEmotion = Emotion.Neutral;
        PlayEmotionLoop(currentEmotion);
    }

    void SetupLoop(AudioSource source, AudioClip clip, float volume)
    {
        if (source != null && clip != null)
        {
            source.clip = clip;
            source.loop = true;
            source.volume = volume;
            source.Play();
        }
    }

    private void PlayEmotionLoop(Emotion emotion)
    {
        print(emotion);
        // Pick a loop clip from the corresponding emotion list.
        AudioClip loopClip = GetRandomEmotionClip(emotion);
        if (loopClip != null)
        {
            crowdSource.clip = loopClip;
            crowdSource.loop = true;
            crowdSource.volume = crowdVolume;
            crowdSource.Play();
        }
    }

    private AudioClip GetRandomEmotionClip(Emotion emotion)
    {
        List<AudioClip> clipList = null;
        switch (emotion)
        {
            case Emotion.Neutral:
                clipList = neutralCrowdClips;
                break;
            case Emotion.Scared:
                clipList = scaredCrowdClips;
                break;
            case Emotion.Positive:
                clipList = positiveCrowdClips;
                break;
        }

        if (clipList != null && clipList.Count > 0)
        {
            int index = Random.Range(0, clipList.Count);
            return clipList[index];
        }
        return null;
    }

    /// <summary>
    /// Call this to set the global emotion state of the crowd.
    /// It will fade from the current emotion loop to the new one.
    /// </summary>
    public void SetCrowdEmotion(Emotion newEmotion, float fadeTime = 1f)
    {
        print("SetCrowdEmotion with " + newEmotion);
        if (newEmotion == currentEmotion) return;

        StartCoroutine(FadeCrowdEmotion(newEmotion, fadeTime));
    }

    private IEnumerator FadeCrowdEmotion(Emotion newEmotion, float fadeTime)
    {
        // Fade out current
        float startVolume = crowdSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            crowdSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        crowdSource.volume = 0f;

        // Stop old clip
        crowdSource.Stop();

        // Change emotion clip
        currentEmotion = newEmotion;
        PlayEmotionLoop(currentEmotion);

        // Fade in new emotion
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            crowdSource.volume = Mathf.Lerp(0f, crowdVolume, t / fadeTime);
            yield return null;
        }
        crowdSource.volume = crowdVolume;
    }

    /// <summary>
    /// Occasionally play a neutral line on top of the current loop.
    /// </summary>
    public void PlayRandomNeutralLine()
    {
        if (neutralCrowdClips.Count > 0)
        {
            var clip = neutralCrowdClips[Random.Range(0, neutralCrowdClips.Count)];
            if (clip != null && crowdOneShotSource != null)
            {
                crowdOneShotSource.PlayOneShot(clip, 1f);
            }
        }
    }

    /// <summary>
    /// Called when a wave starts. Fade down ambient, fade up urgency.
    /// </summary>
    public void StartWaveAudio(float fadeTime = 1f)
    {
        waveActive = true;
        StartCoroutine(FadeVolume(ambientSourceEngine, ambientVolume, ambientVolume * 0.3f, fadeTime));
        StartCoroutine(FadeVolume(ambientSourceWater, ambientVolume, ambientVolume * 0.3f, fadeTime));
        StartCoroutine(FadeVolume(ambientSourceSeagulls, ambientVolume, ambientVolume * 0.1f, fadeTime));

        // Fade in urgency
        StartCoroutine(FadeVolume(urgencySourceAlarm, 0f, urgencyVolume, fadeTime));
        StartCoroutine(FadeVolume(urgencySourceFire, 0f, urgencyVolume, fadeTime));
    }

    /// <summary>
    /// Called when a wave ends. Fade up ambient, fade down urgency.
    /// </summary>
    public void EndWaveAudio(float fadeTime = 1f)
    {
        waveActive = false;
        StartCoroutine(FadeVolume(ambientSourceEngine, ambientSourceEngine.volume, ambientVolume, fadeTime));
        StartCoroutine(FadeVolume(ambientSourceWater, ambientSourceWater.volume, ambientVolume, fadeTime));
        StartCoroutine(FadeVolume(ambientSourceSeagulls, ambientSourceSeagulls.volume, ambientVolume, fadeTime));

        // Fade out urgency
        StartCoroutine(FadeVolume(urgencySourceAlarm, urgencySourceAlarm.volume, 0f, fadeTime));
        StartCoroutine(FadeVolume(urgencySourceFire, urgencySourceFire.volume, 0f, fadeTime));
    }

    /// <summary>
    /// Fade an AudioSource volume from startVol to endVol over fadeTime seconds.
    /// </summary>
    private IEnumerator FadeVolume(AudioSource source, float startVol, float endVol, float fadeTime)
    {
        if (source == null) yield break;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, endVol, elapsed / fadeTime);
            yield return null;
        }
        source.volume = endVol;
    }

    public void PlayVoiceLine(AudioClip clip)
    {
        if (clip != null && crowdOneShotSource != null)
        {
            crowdOneShotSource.PlayOneShot(clip);
        }
    }

}
