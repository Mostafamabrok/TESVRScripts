using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class CubeTouchMoveXRI : MonoBehaviour
{
    private Vector3 originalPosition;
    public Vector3 separatedOffset = new Vector3(1f, 0f, 0f);
    public float staggerDelay = 0f;  // << New — per cube delay

    private static bool isSeparated = false;  // Shared by all cubes
    private static bool isMoving = false;     // Prevent multiple triggers

    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private XRBaseInteractable interactable;

    private static List<CubeTouchMoveXRI> allCubes = new List<CubeTouchMoveXRI>();

    void Awake()
    {
        originalPosition = transform.position;

        interactable = GetComponent<XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogError("Missing XRBaseInteractable on object: " + gameObject.name);
        }
        else
        {
            interactable.selectEntered.AddListener(OnSelectEntered);
        }

        // Register this cube
        allCubes.Add(this);
    }

    private void OnDestroy()
    {
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(OnSelectEntered);
        }

        // Unregister this cube
        allCubes.Remove(this);
    }

    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (!isMoving)
        {
            // Trigger all cubes to move
            foreach (var cube in allCubes)
            {
                Vector3 targetPos = isSeparated ? cube.originalPosition : cube.originalPosition + cube.separatedOffset;
                cube.StartCoroutine(cube.MoveToPosition(targetPos, cube.staggerDelay));
            }

            // Flip shared state
            isSeparated = !isSeparated;
        }
    }

    System.Collections.IEnumerator MoveToPosition(Vector3 targetPosition, float delay)
    {
        isMoving = true;

        // Wait for this cube's personal stagger delay
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsedTime = 0f;
        float duration = 0.5f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float curveValue = easingCurve.Evaluate(t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        isMoving = false;
    }
}
