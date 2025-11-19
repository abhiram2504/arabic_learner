using UnityEngine;
using UnityEngine.Events;

public class TriggerOnCollision : MonoBehaviour
{
    [SerializeField] private UnityEvent onCollision;

    private void OnCollisionEnter(Collision collision)
    {
        onCollision.Invoke();
    }
}
