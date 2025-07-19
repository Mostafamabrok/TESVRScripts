using UnityEngine;

// This attribute ensures that an AudioSource component is available on the GameObject.
// It will automatically add one if it's missing, preventing potential errors.
[RequireComponent(typeof(AudioSource))]
public class DoorHingeRotation : MonoBehaviour
{

    public enum DoorSide { Left, Right }
    public DoorSide doorSide = DoorSide.Right;
    public float openAngle = 90f;
    public float duration = 1.5f;

    [Header("Audio Settings")]
    [Tooltip("The delay in seconds after the door starts closing before the sound plays.")]
    public float closingSoundDelay = 0.1f; // Public variable for the delay

    [Tooltip("The random pitch variation. 0.1 means pitch will be between 0.9 and 1.1.")]
    public float pitchVariation = 0.1f; // Public variable for pitch randomization

    private Quaternion closedRot;
    private Quaternion openRot;
    private Quaternion startRot;  // The rotation when the animation begins
    private Quaternion targetRot;

    private bool isOpen = false;
    private bool animating = false;
    private float timer = 0f;

    private AudioSource audioSource; // Reference to the AudioSource component

    void Start()
    {
        // Get the AudioSource component attached to this GameObject
        audioSource = GetComponent<AudioSource>();

        // Store the initial (closed) rotation
        closedRot = transform.localRotation;

        // Calculate the open rotation based on the side and angle
        float angle = (doorSide == DoorSide.Left) ? -openAngle : openAngle;
        // Apply rotation relative to the initial closed state
        openRot = closedRot * Quaternion.Euler(0, angle, 0);
    }

    void Update()
    {
        // Only run the animation logic if the door is supposed to be moving
        if (!animating) return;

        // Increment the timer and calculate the interpolation factor 't'
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        // Use a smoothstep function for a nice ease-in and ease-out effect
        float easedT = t * t * (3f - 2f * t);

        // Smoothly interpolate from the rotation we started at towards the target rotation
        transform.localRotation = Quaternion.Slerp(startRot, targetRot, easedT);

        // Check if the animation has finished
        if (t >= 1f)
        {
            animating = false;
            // The door's state has now officially changed (it's fully open or fully closed)
            isOpen = !isOpen;
        }
    }

    /// <summary>
    /// Toggles the door between its open and closed states, playing a sound.
    /// </summary>
    public void ToggleDoor()
    {
        // Don't start a new animation if one is already playing
        if (animating) return;

        // --- Start the animation ---
        animating = true;
        timer = 0f;
        startRot = transform.localRotation; // Capture the current rotation as the starting point

        // Determine the target rotation and handle audio
        if (isOpen)
        {
            // --- DOOR IS CLOSING ---
            targetRot = closedRot;
            // Call the sound playing method with the specified delay
            Invoke(nameof(PlayClosingSound), closingSoundDelay);
        }
        else
        {
            // --- DOOR IS OPENING ---
            targetRot = openRot;
            // No sound plays when opening
        }
    }

    /// <summary>
    /// Plays the audio source's clip with a random pitch. Called by Invoke.
    /// </summary>
    private void PlayClosingSound()
    {
        // Set a random pitch before playing to make the sound less repetitive
        audioSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
        audioSource.Play();
    }
}
