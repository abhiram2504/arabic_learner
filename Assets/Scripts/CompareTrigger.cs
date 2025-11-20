using UnityEngine;
using UnityEngine.Events;

public class CompareTrigger : MonoBehaviour
{
    [Header("Only trigger when THIS object collides with this target")]
    public GameObject targetObject;

    [Header("How many collisions are required before triggering?")]
    public int requiredCollisions = 3;

    [SerializeField] private UnityEvent onCollision;

    private int currentCount = 0;

    private void OnCollisionEnter(Collision collision)
    {
        // Only count collisions with the selected target object
        if (collision.gameObject == targetObject)
        {
            currentCount++;

            if (currentCount > requiredCollisions)
            {
                onCollision.Invoke();
                currentCount = 0; // reset (remove this line if you don't want reset)
            }
        }
    }
}
