using UnityEngine;
using System.Collections;

[System.Serializable]
public class NarrationInstruction
{
    [Tooltip("The audio clip to play for this instruction")]
    public AudioClip audioClip;
    
    [Tooltip("Text version of the instruction (for debugging/logging)")]
    [TextArea(2, 4)]
    public string instructionText;
    
    [Tooltip("Delay before playing this instruction (in seconds)")]
    public float delayBeforePlaying = 0f;
    
    [Tooltip("Whether to wait for this audio to finish before continuing")]
    public bool waitForCompletion = true;
}

public class SceneNarrator : MonoBehaviour
{
    [Header("Narration Settings")]
    [Tooltip("List of instructions to play when the scene starts")]
    public NarrationInstruction[] instructions;
    
    [Tooltip("Play instructions automatically when scene starts")]
    public bool playOnStart = true;
    
    [Tooltip("Delay before starting narration (in seconds)")]
    public float startDelay = 1f;
    
    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Volume for narration (0-1)")]
    public float narrationVolume = 1f;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = true;
    
    private AudioSource audioSource;
    private bool isPlaying = false;
    private int currentInstructionIndex = 0;
    
    void Awake()
    {
        // Create audio source for narration
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = narrationVolume;
    }
    
    void Start()
    {
        if (playOnStart)
        {
            StartCoroutine(StartNarrationWithDelay());
        }
    }
    
    IEnumerator StartNarrationWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        PlayInstructions();
    }
    
    /// <summary>
    /// Start playing all instructions from the beginning
    /// </summary>
    public void PlayInstructions()
    {
        if (instructions == null || instructions.Length == 0)
        {
            if (showDebugMessages)
                Debug.LogWarning("SceneNarrator: No instructions configured!");
            return;
        }
        
        if (isPlaying)
        {
            if (showDebugMessages)
                Debug.LogWarning("SceneNarrator: Narration already playing!");
            return;
        }
        
        currentInstructionIndex = 0;
        StartCoroutine(PlayInstructionsCoroutine());
    }
    
    /// <summary>
    /// Play a specific instruction by index
    /// </summary>
    public void PlayInstruction(int index)
    {
        if (instructions == null || index < 0 || index >= instructions.Length)
        {
            if (showDebugMessages)
                Debug.LogWarning($"SceneNarrator: Invalid instruction index: {index}");
            return;
        }
        
        StartCoroutine(PlaySingleInstruction(instructions[index]));
    }
    
    /// <summary>
    /// Stop the current narration
    /// </summary>
    public void StopNarration()
    {
        StopAllCoroutines();
        isPlaying = false;
        
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        
        if (showDebugMessages)
            Debug.Log("SceneNarrator: Narration stopped");
    }
    
    IEnumerator PlayInstructionsCoroutine()
    {
        isPlaying = true;
        
        for (int i = 0; i < instructions.Length; i++)
        {
            currentInstructionIndex = i;
            NarrationInstruction instruction = instructions[i];
            
            if (instruction.audioClip == null)
            {
                if (showDebugMessages)
                    Debug.LogWarning($"SceneNarrator: Instruction {i} has no audio clip, skipping...");
                continue;
            }
            
            // Wait for delay before playing
            if (instruction.delayBeforePlaying > 0)
            {
                yield return new WaitForSeconds(instruction.delayBeforePlaying);
            }
            
            // Play the instruction
            yield return StartCoroutine(PlaySingleInstruction(instruction));
        }
        
        isPlaying = false;
        
        if (showDebugMessages)
            Debug.Log("SceneNarrator: All instructions completed");
    }
    
    IEnumerator PlaySingleInstruction(NarrationInstruction instruction)
    {
        if (instruction.audioClip == null)
            yield break;
        
        if (showDebugMessages && !string.IsNullOrEmpty(instruction.instructionText))
        {
            Debug.Log($"SceneNarrator: {instruction.instructionText}");
        }
        
        // Play audio using direct audio source (more flexible for custom clips)
        if (audioSource != null)
        {
            audioSource.clip = instruction.audioClip;
            
            // Apply master volume from AudioManager if available
            if (AudioManager.instance != null)
            {
                audioSource.volume = narrationVolume * AudioManager.instance.GetMasterVolume();
            }
            else
            {
                audioSource.volume = narrationVolume;
            }
            
            audioSource.Play();
            
            // Wait for audio to finish if required
            if (instruction.waitForCompletion)
            {
                while (audioSource.isPlaying)
                {
                    yield return null;
                }
            }
        }
        else
        {
            // Fallback: if no audio source, just wait for clip duration
            if (showDebugMessages)
                Debug.LogWarning("SceneNarrator: No audio source available!");
            
            if (instruction.waitForCompletion)
            {
                yield return new WaitForSeconds(instruction.audioClip.length);
            }
        }
    }
    
    /// <summary>
    /// Check if narration is currently playing
    /// </summary>
    public bool IsPlaying()
    {
        return isPlaying;
    }
    
    /// <summary>
    /// Get the current instruction index
    /// </summary>
    public int GetCurrentInstructionIndex()
    {
        return currentInstructionIndex;
    }
}

