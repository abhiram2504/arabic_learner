using UnityEngine;
using UnityEngine.SceneManagement;
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
            collision.gameObject.SetActive(false);
            remainingSolids.Remove(collision.gameObject);

            Debug.Log("Collected SOLID: " + collision.gameObject.name);
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
        return pourZone.bounds.Contains(item.transform.position);
    }

    void CheckIfFinished()
    {
        if (remainingSolids.Count == 0 && remainingPourables.Count == 0)
        {
            Debug.Log("ALL ITEMS COMPLETE!");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
