using UnityEngine;
using System.Collections;

public class HoverLabel : MonoBehaviour
{
    public CanvasGroup labelCanvas; // assign this in inspector
    public float fadeDuration = 0.3f;

    private Coroutine fadeRoutine;

    void Start()
    {
        if (labelCanvas != null)
        {
            labelCanvas.alpha = 0f; // hide initially
            labelCanvas.interactable = false;
            labelCanvas.blocksRaycasts = false;
        }
    }

    public void ShowLabel()
    {
        StartFade(1f);
    }

    public void HideLabel()
    {
        StartFade(0f);
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(targetAlpha));
    }

    private IEnumerator FadeTo(float target)
    {
        float start = labelCanvas.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            labelCanvas.alpha = Mathf.Lerp(start, target, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }

        labelCanvas.alpha = target;
    }
}
