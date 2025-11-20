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
    public AudioClip collectClip;
    public float collectVolume = 1f;

    [Header("Event invoked when all items are completed (OUTPUT LIST)")]
    public UnityEvent onAllItemsCompleted;

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
        // ✅ FIX: Always resolve the ROOT food object
        GameObject rootItem = collision.gameObject.transform.root.gameObject;

        if (remainingSolids.Contains(rootItem))
        {
            string name = rootItem.name;
            Vector3 soundPos = rootItem.transform.position;

            rootItem.SetActive(false);
            remainingSolids.Remove(rootItem);

            Debug.Log("Collected SOLID: " + name);

            if (collectClip != null)
                AudioSource.PlayClipAtPoint(collectClip, soundPos, collectVolume);

            CheckIfFinished();
        }
    }

    void CheckPour(GameObject item)
    {
        if (!IsInsidePourZone(item))
            return;

        Vector3 rot = item.transform.eulerAngles;
        rot.x = NormalizeAngle(rot.x);
        rot.y = NormalizeAngle(rot.y);
        rot.z = NormalizeAngle(rot.z);

        bool rotatedSideways =
            Mathf.Abs(rot.x) > rotationThreshold ||
            Mathf.Abs(rot.z) > rotationThreshold;

        if (rotatedSideways)
        {
            Debug.Log("POURED (rotation threshold): " + item.name);

            if (collectClip != null)
                AudioSource.PlayClipAtPoint(collectClip, item.transform.position, collectVolume);

            remainingPourables.Remove(item);
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
        // ✅ FIX: Use root position so swapped visuals don't matter
        return pourZone != null && pourZone.bounds.Contains(item.transform.root.position);
    }

    void CheckIfFinished()
    {
        if (remainingSolids.Count == 0 && remainingPourables.Count == 0)
        {
            Debug.Log("ALL ITEMS COMPLETE! Invoking output list.");
            onAllItemsCompleted.Invoke();
        }
    }

    public bool IsComplete()
    {
        return remainingSolids.Count == 0 && remainingPourables.Count == 0;
    }

    public void Reset()
    {
        // Reactivate all solid ingredients that were deactivated
        foreach (var item in solidIngredients)
        {
            if (item != null && !item.activeSelf)
            {
                item.SetActive(true);
            }
        }

        // Reset the tracking sets
        remainingSolids = new HashSet<GameObject>(solidIngredients);
        remainingPourables = new HashSet<GameObject>(pourIngredients);

        Debug.Log("FoodItems reset: All ingredients reactivated and tracking reset.");
    }
}
