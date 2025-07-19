// Filename: SkyAssembleOrdered.cs
//
// This script animates a collection of objects into place from an offset.
//
// --- DIAGNOSIS & FIX ---
// The audio trigger logic has been changed from a time-based delay to a
// position-based trigger. The sound now plays when the object's local Y
// position crosses a specific, adjustable threshold.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyAssembleOrdered : MonoBehaviour
{
    [Header("Animation Settings")]
    public float offsetY = 300f;
    [Tooltip("Fraction of the animation duration that parts overlap (0 = sequential, 1 = all at once)")]
    [Range(0f, 1f)] public float overlapFactor = 0.5f;
    [Tooltip("Time in seconds for each part to complete its animation")] public float duration = 2f;

    [Header("Audio Settings")]
    [Tooltip("The Y-position at which the audio will trigger as the object falls.")]
    public float audioTriggerY = 10f;
    [Tooltip("Amount to vary the audio pitch randomly (e.g. 0.1 for ±10% variation)")] public float pitchVariance = 0.1f;

    // Static list to keep track of all instances for the global trigger
    private static List<SkyAssembleOrdered> instances = new List<SkyAssembleOrdered>();
    private static bool hasAssembled = false; // Prevents re-triggering

    // Component-specific variables
    private Vector3 finalPosition;
    private Vector3 startPosition;
    private bool isAnimating;
    private float animationTimer;

    // Audio
    private AudioSource audioSource;
    private bool audioPlayed;

    // Reference to the other script for coordination
    private LookAtInteractable lookAtScript;

    void Awake()
    {
        lookAtScript = GetComponent<LookAtInteractable>();
        if (lookAtScript != null)
        {
            lookAtScript.enabled = false;
        }

        finalPosition = transform.localPosition;
        startPosition = finalPosition + Vector3.up * offsetY;

        // Immediately move the object to its starting (non-assembled) position on load.
        transform.localPosition = startPosition;

        audioSource = GetComponent<AudioSource>();
        instances.Add(this);
    }

    void OnDestroy()
    {
        instances.Remove(this);
    }

    void Update()
    {
        if (!isAnimating)
            return;

        animationTimer += Time.deltaTime;
        float t = Mathf.Clamp01(animationTimer / duration);
        float easedT = Mathf.SmoothStep(0f, 1f, t);

        transform.localPosition = Vector3.Lerp(startPosition, finalPosition, easedT);

        // Check if the object has passed the Y-threshold to play the audio
        if (!audioPlayed && transform.localPosition.y <= audioTriggerY)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
                audioSource.Play();
            }
            audioPlayed = true; // Mark as played to prevent it from triggering again
        }

        if (t >= 1f)
        {
            isAnimating = false;
        }
    }

    private void StartAnimation()
    {
        transform.localPosition = startPosition;
        isAnimating = true;
        animationTimer = 0f;
        audioPlayed = false;
    }

    public static void TriggerAssembleAll()
    {
        if (instances.Count == 0 || hasAssembled)
            return;

        hasAssembled = true;
        instances[0].StartCoroutine(instances[0].BeginOrderedAssembly());
    }

    private IEnumerator BeginOrderedAssembly()
    {
        foreach (var instance in instances)
        {
            if (instance.lookAtScript != null)
            {
                instance.lookAtScript.enabled = false;
            }
        }

        var sortedInstances = new List<SkyAssembleOrdered>(instances);
        sortedInstances.Sort((a, b) =>
        {
            int xComp = a.finalPosition.x.CompareTo(b.finalPosition.x);
            if (xComp != 0) return xComp;
            float yDiff = Mathf.Abs(a.finalPosition.y - b.finalPosition.y);
            if (yDiff > 0.5f) return a.finalPosition.y.CompareTo(b.finalPosition.y);
            return b.finalPosition.z.CompareTo(a.finalPosition.z);
        });

        int count = sortedInstances.Count;

        float delayBetweenStarts = (count > 1)
            ? duration * (1f - overlapFactor) / (count - 1)
            : 0f;

        for (int i = 0; i < count; i++)
        {
            sortedInstances[i].StartAnimation();
            // Don't wait after starting the very last animation
            if (i < count - 1)
            {
                yield return new WaitForSeconds(delayBetweenStarts);
            }
        }

        // The last animation has just started. Now, just wait for its duration to complete.
        yield return new WaitForSeconds(duration);

        Debug.Log("Assembly complete. Handing control to LookAtInteractable scripts.");
        foreach (var instance in sortedInstances)
        {
            if (instance.lookAtScript != null)
            {
                instance.lookAtScript.enabled = true;
            }
        }
    }
}
