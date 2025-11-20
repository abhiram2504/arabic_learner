using UnityEngine;
using UnityEngine.Events;
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

    [Header("Sound (plays at the completed item's world position)")]
    public AudioClip collectClip; // optional - assign clip in inspector
    public float collectVolume = 1f;

    [Header("Event invoked when all items are completed (OUTPUT LIST)")]
    public UnityEvent onAllItemsCompleted; // editable list of outputs in inspector

    private HashSet<GameObject> remainingSolids;
    private HashSet<GameObject> remainingPourables;

    void Start()
    {
        remainingSolids = new HashSet<GameObject>(solidIngredients);
        remainingPourables = new HashSet<GameObject>(pourIngredients);
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
            Vector3 soundPos = collision.gameObject.transform.position;

            // hide / collect; remove from remaining set
            collision.gameObject.SetActive(false);
            remainingSolids.Remove(collision.gameObject);

            Debug.Log("Collected SOLID: " + name);

            // play sound at the item's world position (if assigned)
            if (collectClip != null)
                AudioSource.PlayClipAtPoint(collectClip, soundPos, collectVolume);

            // Check if all finished and invoke output event if so
            CheckIfFinished();
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

            // play sound at the item's world position (if assigned)
            if (collectClip != null)
                AudioSource.PlayClipAtPoint(collectClip, item.transform.position, collectVolume);

            remainingPourables.Remove(item);

            // Check if all finished and invoke output event if so
            CheckIfFinished();
        }
    }

    float NormalizeAngle(float a)
    {
        if (a > 180f) a -= 360f;
        return a;
    }

    bool IsInsidePourZone(GameObject item)
    {
        return pourZone != null && pourZone.bounds.Contains(item.transform.position);
    }

    void CheckIfFinished()
    {
        if (remainingSolids.Count == 0 && remainingPourables.Count == 0)
        {
            Debug.Log("ALL ITEMS COMPLETE! Invoking output list.");
            onAllItemsCompleted?.Invoke();
        }
    }
}
