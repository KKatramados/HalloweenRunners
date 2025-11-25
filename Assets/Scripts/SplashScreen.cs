using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

// SplashScreen.cs - Shows splash screen(s) before main menu
public class SplashScreen : MonoBehaviour
{
    [Header("Splash Settings")]
    public Image splashImage; // The splash screen image (logo, studio name, etc.)
    public float displayDuration = 3f; // How long to show splash
    public float fadeInDuration = 1f;
    public float fadeOutDuration = 1f;

    [Header("Next Scene")]
    public string nextSceneName = "MainMenu"; // Scene to load after splash

    [Header("Skip Settings")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    private bool isSkipped = false;

    void Start()
    {
        if (splashImage != null)
        {
            StartCoroutine(ShowSplashSequence());
        }
        else
        {
            Debug.LogError("Splash Image not assigned!");
            LoadNextScene();
        }
    }

    void Update()
    {
        // Allow skipping with key or any button press
        if (allowSkip && !isSkipped)
        {
            if (Input.GetKeyDown(skipKey) || Input.anyKeyDown || Input.GetMouseButtonDown(0))
            {
                isSkipped = true;
                StopAllCoroutines();
                LoadNextScene();
            }
        }
    }

    IEnumerator ShowSplashSequence()
    {
        // Start fully transparent
        Color color = splashImage.color;
        color.a = 0f;
        splashImage.color = color;

        // Fade in
        yield return StartCoroutine(FadeIn());

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return StartCoroutine(FadeOut());

        // Load next scene
        LoadNextScene();
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color color = splashImage.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            splashImage.color = color;
            yield return null;
        }

        color.a = 1f;
        splashImage.color = color;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color color = splashImage.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            splashImage.color = color;
            yield return null;
        }

        color.a = 0f;
        splashImage.color = color;
    }

    void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}