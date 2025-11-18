using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    [Header("UI Controls")]
    public Slider masterVolumeSlider;
    public Slider masterPitchSlider;
    public Text volumeValueText;
    public Text pitchValueText;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float masterVolume = 1f;

    [Range(0.1f, 3f)]
    public float masterPitch = 1f;

    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.5f;
    public bool playBackgroundMusicOnStart = true;
    public bool loopBackgroundMusic = true;

    public static AudioManager instance;

    private AudioSource backgroundMusicSource;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize audio sources for each sound
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.vol * masterVolume;
            s.source.pitch = s.pitch * masterPitch;
        }

        // Initialize background music source
        if (backgroundMusic != null)
        {
            backgroundMusicSource = gameObject.AddComponent<AudioSource>();
            backgroundMusicSource.clip = backgroundMusic;
            backgroundMusicSource.volume = backgroundMusicVolume * masterVolume;
            backgroundMusicSource.loop = loopBackgroundMusic;
            backgroundMusicSource.playOnAwake = false; // We'll control when it plays
        }
    }

    void Start()
    {
        SetupUI();
        UpdateAllSounds();

        // Play background music if enabled
        if (playBackgroundMusicOnStart && backgroundMusic != null && backgroundMusicSource != null)
        {
            PlayBackgroundMusic();
        }
    }

    void SetupUI()
    {
        // Setup master volume slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // Setup master pitch slider
        if (masterPitchSlider != null)
        {
            masterPitchSlider.minValue = 0.1f;
            masterPitchSlider.maxValue = 3f;
            masterPitchSlider.value = masterPitch;
            masterPitchSlider.onValueChanged.AddListener(OnMasterPitchChanged);
        }

        UpdateUIValues();
    }

    void OnMasterVolumeChanged(float value)
    {
        masterVolume = value;
        UpdateAllSounds();
        UpdateUIValues();
    }

    void OnMasterPitchChanged(float value)
    {
        masterPitch = value;
        UpdateAllSounds();
        UpdateUIValues();
    }

    void UpdateAllSounds()
    {
        foreach (Sound s in sounds)
        {
            if (s.source != null)
            {
                s.source.volume = s.vol * masterVolume;
                s.source.pitch = s.pitch * masterPitch;
            }
        }

        // Update background music volume
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = backgroundMusicVolume * masterVolume;
        }
    }

    void UpdateUIValues()
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = $"Volume: {(masterVolume * 100):F0}%";
        }

        if (pitchValueText != null)
        {
            pitchValueText.text = $"Pitch: {masterPitch:F2}x";
        }
    }

    // Public methods to play sounds
    public void Play(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        // Update volume and pitch before playing
        s.source.volume = s.vol * masterVolume;
        s.source.pitch = s.pitch * masterPitch;
        s.source.Play();
    }

    public void Stop(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        s.source.Stop();
    }

    public void SetSoundVolume(string soundName, float volume)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        s.vol = Mathf.Clamp01(volume);
        if (s.source != null)
        {
            s.source.volume = s.vol * masterVolume;
        }
    }

    public void SetSoundPitch(string soundName, float pitch)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + soundName + " not found!");
            return;
        }

        s.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
        if (s.source != null)
        {
            s.source.pitch = s.pitch * masterPitch;
        }
    }

    // Get current values
    public float GetMasterVolume()
    {
        return masterVolume;
    }

    public float GetMasterPitch()
    {
        return masterPitch;
    }

    public float GetSoundVolume(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        return s != null ? s.vol : 0f;
    }

    public float GetSoundPitch(string soundName)
    {
        Sound s = System.Array.Find(sounds, sound => sound.name == soundName);
        return s != null ? s.pitch : 1f;
    }

    // Background music methods
    public void PlayBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusic != null)
        {
            if (!backgroundMusicSource.isPlaying)
            {
                backgroundMusicSource.volume = backgroundMusicVolume * masterVolume;
                backgroundMusicSource.Play();
                Debug.Log("Background music started");
            }
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Stop();
            Debug.Log("Background music stopped");
        }
    }

    public void PauseBackgroundMusic()
    {
        if (backgroundMusicSource != null && backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.Pause();
            Debug.Log("Background music paused");
        }
    }

    public void ResumeBackgroundMusic()
    {
        if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
        {
            backgroundMusicSource.UnPause();
            Debug.Log("Background music resumed");
        }
    }

    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        if (backgroundMusicSource != null)
        {
            backgroundMusicSource.volume = backgroundMusicVolume * masterVolume;
        }
    }

    public bool IsBackgroundMusicPlaying()
    {
        return backgroundMusicSource != null && backgroundMusicSource.isPlaying;
    }
}
