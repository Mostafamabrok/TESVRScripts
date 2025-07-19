// Filename: LookAtInteractable.cs
//
// Attach this script to any GameObject you want to move when the player looks at it.
// The script will remain inactive until the '4' key is pressed, after which a 10-second
// countdown begins. Once the countdown is complete, the gaze interaction is enabled.

using System.Collections;
using UnityEngine;

public class LookAtInteractable : MonoBehaviour
{
    [Header("Activation")]
    [Tooltip("The key to press to begin the activation sequence.")]
    public KeyCode activationKey = KeyCode.Alpha4;
    [Tooltip("The delay in seconds after the key is pressed before the script becomes active.")]
    public float activationDelay = 10.0f;

    [Header("Movement Settings")]
    [Tooltip("How far the object should move upwards when looked at.")]
    public float moveDistance = 0.5f;
    [Tooltip("How quickly the object moves to its target position. Higher is faster.")]
    public float moveSpeed = 2.0f;

    [Header("Gaze Detection")]
    [Tooltip("The maximum distance from which the player's gaze can trigger the movement.")]
    public float maxGazeDistance = 10.0f;

    // Private variables to store state
    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isGazedAt = false;
    private bool isFullyActive = false;
    private bool isActivationSequenceStarted = false;
    private Camera mainCamera;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    void Start()
    {
        // Store the initial position of the object.
        originalPosition = transform.position;

        // Calculate the target position based on the moveDistance.
        targetPosition = originalPosition + Vector3.up * moveDistance;

        // Find and cache the main camera for efficiency.
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("LookAtInteractable Error: No main camera found in the scene. Please tag your main camera with 'MainCamera'.");
            enabled = false;
        }
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    void Update()
    {
        // If the main camera isn't set, do nothing.
        if (mainCamera == null) return;

        // Check if the user has started the activation sequence.
        CheckForActivationInput();

        // Only check for gaze if the script is fully active.
        if (isFullyActive)
        {
            CheckForGaze();
        }

        // Always handle movement to ensure it returns to original position if not active.
        HandleMovement();
    }

    /// <summary>
    /// Checks for the activation key press and starts the delay coroutine if pressed.
    /// </summary>
    private void CheckForActivationInput()
    {
        // If the sequence hasn't started and the user presses the key...
        if (!isActivationSequenceStarted && Input.GetKeyDown(activationKey))
        {
            Debug.Log("Activation key pressed. Starting 10-second timer...");
            isActivationSequenceStarted = true;
            StartCoroutine(ActivateAfterDelay());
        }
    }

    /// <summary>
    /// A coroutine that waits for the specified delay before setting the script to active.
    /// </summary>
    private IEnumerator ActivateAfterDelay()
    {
        // Wait for the specified number of seconds.
        yield return new WaitForSeconds(activationDelay);

        // After the delay, set the script to be fully active.
        Debug.Log("Timer finished. Gaze interaction is now active.");
        isFullyActive = true;
    }

    /// <summary>
    /// Performs a raycast from the camera to detect if the player is looking at this object.
    /// </summary>
    private void CheckForGaze()
    {
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGazeDistance))
        {
            isGazedAt = (hit.transform == this.transform);
        }
        else
        {
            isGazedAt = false;
        }
    }

    /// <summary>
    /// Handles the smooth movement of the object using Vector3.Lerp.
    /// </summary>
    private void HandleMovement()
    {
        // The destination is the target position ONLY if the script is active and the object is gazed at.
        // Otherwise, the destination is its original position.
        Vector3 destination = (isFullyActive && isGazedAt) ? targetPosition : originalPosition;

        // Smoothly interpolate the object's position towards the destination.
        transform.position = Vector3.Lerp(transform.position, destination, moveSpeed * Time.deltaTime);
    }

    /// <summary>
    /// (Optional) Draws a debug ray in the Scene view to visualize the gaze.
    /// </summary>
    void OnDrawGizmos()
    {
        if (mainCamera == null) return;

        // Only draw the gizmo if the script is fully active.
        if (isFullyActive)
        {
            Gizmos.color = isGazedAt ? Color.green : Color.red;
            Gizmos.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * maxGazeDistance);
        }
    }
}
