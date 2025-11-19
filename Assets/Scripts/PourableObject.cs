using UnityEngine;

public class PourableObject : MonoBehaviour
{
    [Header("Particle or Visual FX")]
    public ParticleSystem pourParticles;

    [Header("Pour Settings")]
    public float rotationThreshold = 60f; // degrees needed before pouring begins
    public enum Axis { X, Z, Both }
    public Axis checkAxis = Axis.Both;

    [Header("Optional Sound")]
    public AudioSource pourSound;

    private bool isPouring = false;

    void Update()
    {
        if (pourParticles == null)
            return;

        Vector3 rot = transform.eulerAngles;

        // Normalize angles (350° becomes -10°)
        float rx = Normalize(rot.x);
        float rz = Normalize(rot.z);

        bool rotatedEnough = false;

        // Determine which axis to check
        switch (checkAxis)
        {
            case Axis.X:
                rotatedEnough = Mathf.Abs(rx) > rotationThreshold;
                break;
            case Axis.Z:
                rotatedEnough = Mathf.Abs(rz) > rotationThreshold;
                break;
            case Axis.Both:
                rotatedEnough = Mathf.Abs(rx) > rotationThreshold || Mathf.Abs(rz) > rotationThreshold;
                break;
        }

        // Start FX
        if (rotatedEnough && !isPouring)
        {
            pourParticles.Play();
            if (pourSound != null) pourSound.Play();
            isPouring = true;
        }
        // Stop FX
        else if (!rotatedEnough && isPouring)
        {
            pourParticles.Stop();
            if (pourSound != null) pourSound.Stop();
            isPouring = false;
        }
    }

    float Normalize(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }

    public bool IsPouring()
    {
        return isPouring;
    }
}
