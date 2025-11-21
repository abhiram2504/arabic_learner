using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour
{
    [Header("Audio References")]
    public AudioSource audioSource;
    public AudioClip audioClip;

    [Header("Playback Settings")]
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool loop = false;
    public float pitch = 1f;

    [Header("Play Options")]
    public bool playOnSceneStart = false;
    public float delay = 0f; // Delay in seconds before playing

    void Start()
    {
        ApplySettings();

        if (playOnSceneStart)
        {
            if (delay > 0)
                StartCoroutine(PlayWithDelay());
            else
                playAudio();
        }
    }

    // Call this manually or via button
    public void playAudio()
    {
        if (audioSource == null || audioClip == null)
        {
            Debug.LogWarning("AudioSource or AudioClip not assigned.");
            return;
        }

        audioSource.clip = audioClip;
        audioSource.Play();
    }

    IEnumerator PlayWithDelay()
    {
        yield return new WaitForSeconds(delay);
        playAudio();
    }

    void ApplySettings()
    {
        if (audioSource == null) return;

        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.pitch = pitch;
    }
}
