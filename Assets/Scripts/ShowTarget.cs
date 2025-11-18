using UnityEngine;

public class ShowTarget : MonoBehaviour
{
    public GameObject target;

    public void Show()
    {
        if (target != null)
            target.SetActive(true);
    }
}
