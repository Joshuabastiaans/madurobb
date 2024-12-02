using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Voice Lines")]
    public AudioClip tutorialClip;
    public AudioClip waveStartClip;
    public AudioClip waveCompleteClip;

    [Header("Player Activity Clips")]
    public AudioClip fireExtinguishedClip;
    public AudioClip inactivityClip;

    public void PlayVoiceLine(AudioClip clip)
    {
        if (clip != null)
        {
            // Start a coroutine to play the clip
            StartCoroutine(PlayClipCoroutine(clip));
        }
        else
        {
            Debug.LogWarning("AudioManager: Attempted to play a null AudioClip.");
        }
    }

    private IEnumerator PlayClipCoroutine(AudioClip clip)
    {
        // Create a new GameObject to hold the AudioSource
        GameObject audioObject = new GameObject("AudioSource_" + clip.name);
        audioObject.transform.SetParent(transform);

        // Add an AudioSource component
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = 1f; // Adjust as needed

        // Play the clip
        audioSource.Play();

        // Wait until the clip has finished playing
        yield return new WaitForSeconds(clip.length);

        // Destroy the AudioSource component and its GameObject
        Destroy(audioObject);
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            // Start a coroutine to play the clip
            StartCoroutine(PlayClipCoroutine(clip));
        }
        else
        {
            Debug.LogWarning("AudioManager: Attempted to play a null AudioClip.");
        }
    }
}
