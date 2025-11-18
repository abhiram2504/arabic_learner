using UnityEngine;

public class HideTarget : MonoBehaviour
{
    public GameObject target;

    public void Hide()
    {
        if (target != null)
            target.SetActive(false);
    }
}
