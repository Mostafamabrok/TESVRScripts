using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage; // Fullscreen UI Image with black color and alpha 1
    public float fadeDuration = 1.5f;

    void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("FadeController: No fadeImage assigned.");
            return;
        }

        fadeImage.gameObject.SetActive(true);
        fadeImage.color = Color.black;

        // Fade in on scene start
        StartCoroutine(FadeIn());
    }

    public void FadeAndReloadScene()
    {
        StartCoroutine(FadeThenReload());
    }

    IEnumerator FadeThenReload()
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator FadeIn()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = 1f - (timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.clear;
        fadeImage.gameObject.SetActive(false); // Optional: hide after fade-in
    }

    IEnumerator FadeOut()
    {
        fadeImage.gameObject.SetActive(true);
        float timer = 0f;
        while (timer < fadeDuration)
        {
            float alpha = timer / fadeDuration;
            fadeImage.color = new Color(0, 0, 0, alpha);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeImage.color = Color.black;
    }
}
