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
    private bool hasEntered = false; // prevents counting multiple times per hit

    private void OnCollisionEnter(Collision collision)
    {
        // Only count collisions with the selected target object
        if (collision.gameObject == targetObject && !hasEntered)
        {
            hasEntered = true;  // lock until exit

            currentCount++;

            if (currentCount >= requiredCollisions)
            {
                onCollision.Invoke();
                currentCount = 0; // reset
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Unlock counting once the objects separate
        if (collision.gameObject == targetObject)
        {
            hasEntered = false;
        }
    }
}
