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

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayVoiceLine(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
