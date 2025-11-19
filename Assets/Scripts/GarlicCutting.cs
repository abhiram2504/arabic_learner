using UnityEngine;
using System.Collections;

public class GarlicCutting : MonoBehaviour
{
    [Header("Garlic Settings")]
    [Tooltip("The whole garlic GameObject (will be hidden when cut)")]
    public GameObject wholeGarlic;
    
    [Tooltip("The cut garlic GameObject (will be shown when cutting is complete)")]
    public GameObject cutGarlic;
    
    [Tooltip("Number of cuts required to complete the task")]
    public int requiredCuts = 5;
    
    [Header("Knife Settings")]
    [Tooltip("Tag for the knife object (or leave empty to detect any object)")]
    public string knifeTag = "Knife";
    
    [Tooltip("Minimum velocity required for a cut to count")]
    public float minCutVelocity = 0.5f;
    
    [Header("Visual Feedback")]
    [Tooltip("Particle effect to play when cutting (optional)")]
    public ParticleSystem cutParticleEffect;
    
    [Tooltip("Sound effect to play when cutting (optional)")]
    public AudioClip cutSound;
    
    [Header("Narration")]
    [Tooltip("Reference to SceneNarrator for instructions")]
    public SceneNarrator narrator;
    
    [Tooltip("Instruction index to play when cutting starts")]
    public int cuttingInstructionIndex = 0;
    
    [Header("Debug")]
    [Tooltip("Show debug messages")]
    public bool showDebugMessages = true;
    
    private int currentCuts = 0;
    private bool isCutting = false;
    private bool isComplete = false;
    private AudioSource audioSource;
    private Rigidbody knifeRigidbody;
    private Vector3 lastKnifePosition;
    private float lastCutTime = 0f;
    private float cutCooldown = 0.3f; // Prevent multiple cuts from same movement
    
    void Start()
    {
        // Initialize garlic states
        if (wholeGarlic != null)
        {
            wholeGarlic.SetActive(true);
        }
        
        if (cutGarlic != null)
        {
            cutGarlic.SetActive(false);
        }
        
        // Set up audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.7f;
        
        // Find narrator if not assigned
        if (narrator == null)
        {
            narrator = FindObjectOfType<SceneNarrator>();
        }
        
        // Play initial instruction
        if (narrator != null && cuttingInstructionIndex >= 0)
        {
            StartCoroutine(PlayCuttingInstruction());
        }
    }
    
    IEnumerator PlayCuttingInstruction()
    {
        // Wait a moment before playing instruction
        yield return new WaitForSeconds(1f);
        
        if (narrator != null)
        {
            narrator.PlayInstruction(cuttingInstructionIndex);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is a knife
        if (isComplete) return;
        
        GameObject knife = other.gameObject;
        
        // Check tag if specified
        if (!string.IsNullOrEmpty(knifeTag) && !knife.CompareTag(knifeTag))
        {
            return;
        }
        
        // Get knife velocity
        Rigidbody rb = knife.GetComponent<Rigidbody>();
        float velocity = 0f;
        
        if (rb != null)
        {
            velocity = rb.velocity.magnitude;
        }
        else
        {
            // Calculate velocity manually if no rigidbody
            velocity = Vector3.Distance(knife.transform.position, lastKnifePosition) / Time.deltaTime;
        }
        
        // Check if velocity is sufficient for a cut
        if (velocity >= minCutVelocity && Time.time - lastCutTime > cutCooldown)
        {
            PerformCut(knife);
        }
        
        lastKnifePosition = knife.transform.position;
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Also check collisions (for physics-based cutting)
        if (isComplete) return;
        
        GameObject knife = collision.gameObject;
        
        // Check tag if specified
        if (!string.IsNullOrEmpty(knifeTag) && !knife.CompareTag(knifeTag))
        {
            return;
        }
        
        // Get knife velocity
        Rigidbody rb = knife.GetComponent<Rigidbody>();
        float velocity = 0f;
        
        if (rb != null)
        {
            velocity = rb.velocity.magnitude;
        }
        
        // Check if velocity is sufficient for a cut
        if (velocity >= minCutVelocity && Time.time - lastCutTime > cutCooldown)
        {
            PerformCut(knife);
        }
    }
    
    void PerformCut(GameObject knife)
    {
        lastCutTime = Time.time;
        currentCuts++;
        
        if (showDebugMessages)
        {
            Debug.Log($"Garlic cut! ({currentCuts}/{requiredCuts})");
        }
        
        // Play cut sound
        if (cutSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(cutSound);
        }
        
        // Play particle effect
        if (cutParticleEffect != null)
        {
            cutParticleEffect.Play();
        }
        
        // Check if cutting is complete
        if (currentCuts >= requiredCuts)
        {
            CompleteCutting();
        }
    }
    
    void CompleteCutting()
    {
        if (isComplete) return;
        
        isComplete = true;
        
        if (showDebugMessages)
        {
            Debug.Log("Garlic cutting complete! Transforming to cut garlic...");
        }
        
        // Hide whole garlic and show cut garlic
        if (wholeGarlic != null)
        {
            wholeGarlic.SetActive(false);
        }
        
        if (cutGarlic != null)
        {
            cutGarlic.SetActive(true);
        }
        
        // Optional: Play completion sound or effect
        StartCoroutine(OnCuttingComplete());
    }
    
    IEnumerator OnCuttingComplete()
    {
        // Small delay before any completion effects
        yield return new WaitForSeconds(0.5f);
        
        // You can add completion effects here (particles, sounds, etc.)
        if (showDebugMessages)
        {
            Debug.Log("Garlic has been successfully cut into tiny pieces!");
        }
    }
    
    /// <summary>
    /// Get the current number of cuts
    /// </summary>
    public int GetCurrentCuts()
    {
        return currentCuts;
    }
    
    /// <summary>
    /// Get the required number of cuts
    /// </summary>
    public int GetRequiredCuts()
    {
        return requiredCuts;
    }
    
    /// <summary>
    /// Check if cutting is complete
    /// </summary>
    public bool IsComplete()
    {
        return isComplete;
    }
    
    /// <summary>
    /// Reset the cutting state (useful for restarting)
    /// </summary>
    public void ResetCutting()
    {
        currentCuts = 0;
        isComplete = false;
        
        if (wholeGarlic != null)
        {
            wholeGarlic.SetActive(true);
        }
        
        if (cutGarlic != null)
        {
            cutGarlic.SetActive(false);
        }
    }
}

