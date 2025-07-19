using UnityEngine;
using System.Collections;

// This attribute ensures that an AudioSource component is available on the GameObject.
// It will automatically add one if it's missing, preventing potential errors.
[RequireComponent(typeof(AudioSource))]
public class ManualRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f;   // Degrees per second applied to target
    public float smoothSpeed = 5f;      // How quickly actual rotation catches up

    [Header("Audio Settings")]
    [Tooltip("How long in seconds it takes for the audio to fade in and out.")]
    public float fadeDuration = 0.5f;   // Public variable to control fade speed

    private Quaternion targetRotation;
    private AudioSource audioSource;    // Reference to the AudioSource component

    // --- Variables for Audio Fading ---
    private float maxVolume;
    private Coroutine audioFadeCoroutine;
    private bool wasActivelyRotating = false;

    void Start()
    {
        // Initialize target to current orientation
        targetRotation = transform.rotation;

        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        // Store the original volume from the Inspector
        maxVolume = audioSource.volume;
        // Start with the volume at 0 so it can fade in
        audioSource.volume = 0f;

        // Play the source. It will be silent until we fade the volume up.
        audioSource.Play();
    }

    void Update()
    {
        // This flag will track if we are giving any rotation commands this frame.
        bool isActivelyRotating = false;

        // Only adjust target when holding 3
        if (Input.GetKey(KeyCode.Alpha3))
        {
            float rx = 0f, ry = 0f, rz = 0f;

            // Check for each rotation key. If one is pressed, set our flag to true.
            if (Input.GetKey(KeyCode.U)) { rx += 1f; isActivelyRotating = true; }
            if (Input.GetKey(KeyCode.O)) { rx -= 1f; isActivelyRotating = true; }
            if (Input.GetKey(KeyCode.J)) { ry += 1f; isActivelyRotating = true; }
            if (Input.GetKey(KeyCode.L)) { ry -= 1f; isActivelyRotating = true; }
            if (Input.GetKey(KeyCode.K)) { rz += 1f; isActivelyRotating = true; }
            if (Input.GetKey(KeyCode.I)) { rz -= 1f; isActivelyRotating = true; }

            // Only apply rotation if there was input
            if (isActivelyRotating)
            {
                Vector3 deltaEuler = new Vector3(rx, ry, rz) * rotationSpeed * Time.deltaTime;
                // Apply to target in local space
                targetRotation *= Quaternion.Euler(deltaEuler);
            }
        }

        // --- Audio Fade Control Logic ---
        // Check if the rotation state has changed since the last frame.
        if (isActivelyRotating != wasActivelyRotating)
        {
            // Stop any fade that is currently running to avoid conflicts.
            if (audioFadeCoroutine != null)
            {
                StopCoroutine(audioFadeCoroutine);
            }

            // Start a new fade coroutine based on the new state.
            if (isActivelyRotating)
            {
                // Fade In
                audioFadeCoroutine = StartCoroutine(FadeAudio(maxVolume, fadeDuration));
            }
            else
            {
                // Fade Out
                audioFadeCoroutine = StartCoroutine(FadeAudio(0f, fadeDuration));
            }
        }

        // Update the state for the next frame.
        wasActivelyRotating = isActivelyRotating;

        // Smoothly interpolate actual rotation toward target, regardless of input.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            smoothSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Coroutine to smoothly change the audio volume over a set duration.
    /// </summary>
    /// <param name="targetVolume">The volume to fade to.</param>
    /// <param name="duration">The time the fade should take.</param>
    private IEnumerator FadeAudio(float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float time = 0;

        while (time < duration)
        {
            // Increment time and calculate the new volume using linear interpolation.
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null; // Wait for the next frame
        }

        // Ensure the final volume is set precisely.
        audioSource.volume = targetVolume;
    }
}
