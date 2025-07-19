using UnityEngine;

[ExecuteAlways]
public class Billboard : MonoBehaviour
{
    [Header("Smoothing Settings")]
    public float smoothSpeed = 10f; // Higher = snappier, lower = more lag

    private Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;
        if (cam == null)
            return;

        // Compute the rotation that looks at the camera
        Quaternion targetRot = Quaternion.LookRotation(transform.position - cam.transform.position);
        // Smoothly rotate toward that rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * smoothSpeed);
    }
}
