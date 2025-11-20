using UnityEngine;
using UnityEngine.Events;

public class TriggerOnCollisionCount : MonoBehaviour
{
    [Header("How many collisions are required?")]
    public int requiredCollisions = 3;

    [Header("Event invoked when collision count is reached")]
    public UnityEvent onCollisionCountReached;

    private int collisionCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        collisionCount++;
        // Debug.Log("Collision count: " + collisionCount);

        if (collisionCount >= requiredCollisions)
        {
            onCollisionCountReached.Invoke();
            collisionCount = 0; // reset after triggering (change if you don’t want reset)
        }
    }
}
