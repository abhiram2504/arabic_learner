using UnityEngine;

public class BlenderZone : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;       // Drag an AudioSource here
    public AudioClip blendSound;          // Drag your blender sound here

    [Header("Settings")]
    public string ingredientTag = "Ingredient";  // Tag for objects that should disappear

    private void OnTriggerEnter(Collider other)
    {
        // Check if this object is an ingredient
        if (other.CompareTag(ingredientTag))
        {
            // Play the blend sound
            if (audioSource != null && blendSound != null)
                audioSource.PlayOneShot(blendSound);

            // Destroy the ingredient
            Destroy(other.gameObject);
        }
    }
}