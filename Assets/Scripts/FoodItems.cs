using UnityEngine;
using System.Collections.Generic;

public class FoodItems : MonoBehaviour
{
    [Header("SOLID ingredients that must collide into dish")]
    public List<GameObject> solidIngredients;

    [Header("POURABLE ingredients (salt shaker, pepper, etc.)")]
    public List<GameObject> pourIngredients;

    [Header("Pour detection zone (trigger above dish)")]
    public Collider pourZone;

    [Header("Rotation needed to count as pouring (degrees)")]
    public float rotationThreshold = 60f;

    [Header("Audio (plays each time an item is completed)")]
    public AudioSource audioSource;      // optional, assign an AudioSource
    public AudioClip collectClip;        // optional, assign the sound to play

    // Public output list recording names of items as they are completed
    [Header("Output log (records completed items in order)")]
    public List<string> outputLog = new List<string>();

    private HashSet<GameObject> remainingSolids;
    private HashSet<GameObject> remainingPourables;

    void Start()
    {
        remainingSolids = new HashSet<GameObject>(solidIngredients);
        remainingPourables = new HashSet<GameObject>(pourIngredients);

        // If no audioSource assigned, try to get one on the same GameObject
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        foreach (var item in new List<GameObject>(remainingPourables))
        {
            CheckPour(item);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (remainingSolids.Contains(collision.gameObject))
        {
            string name = collision.gameObject.name;

            // Hide / collect; remove from remaining set
            collision.gameObject.SetActive(false);
            remainingSolids.Remove(collision.gameObject);

            Debug.Log("Collected SOLID: " + name);

            // record and play sound
            RecordCompletion(name);
            PlayCollectSound();
        }
    }

    void CheckPour(GameObject item)
    {
        // must be inside pour zone
        if (!IsInsidePourZone(item))
            return;

        // --- ROTATION-BASED POUR CHECK ---
        Vector3 rot = item.transform.eulerAngles;

        // Normalize so 350° becomes -10° etc.
        rot.x = NormalizeAngle(rot.x);
        rot.y = NormalizeAngle(rot.y);
        rot.z = NormalizeAngle(rot.z);

        bool rotatedSideways =
            Mathf.Abs(rot.x) > rotationThreshold ||
            Mathf.Abs(rot.z) > rotationThreshold;

        if (rotatedSideways)
        {
            Debug.Log("POURED (rotation threshold): " + item.name);

            // Record and play sound BEFORE removing (so name exists)
            RecordCompletion(item.name);
            PlayCollectSound();

            remainingPourables.Remove(item);
        }
    }

    float NormalizeAngle(float a)
    {
        if (a > 180f) a -= 360f;
        return a;
    }

    bool IsInsidePourZone(GameObject item)
    {
        // Simple position-in-bounds check (works with your current setup)
        return pourZone != null && pourZone.bounds.Contains(item.transform.position);
    }

    // Adds a unique entry to the output log (keeps duplicates if desired)
    void RecordCompletion(string itemName)
    {
        // If you want duplicates allowed, just do outputLog.Add(itemName);
        // Current behavior: record every completion (including duplicates).
        outputLog.Add(itemName);
    }

    void PlayCollectSound()
    {
        if (audioSource != null && collectClip != null)
        {
            audioSource.PlayOneShot(collectClip);
        }
    }
}
