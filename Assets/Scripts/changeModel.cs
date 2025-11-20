using UnityEngine;

public class PrefabSwapper : MonoBehaviour
{
    public GameObject targetPrefab;

    public void SwapPrefab()
    {
        if (targetPrefab == null) return;

        // Save current transform
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;
        Transform parent = transform.parent;

        // Destroy current GameObject
        Destroy(gameObject);

        // Instantiate target prefab at the same position/rotation
        GameObject newObj = Instantiate(targetPrefab, pos, rot, parent);
    }
}