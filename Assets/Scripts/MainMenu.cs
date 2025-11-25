using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// MainMenu.cs - Main menu screen with buttons
public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject creditsPanel;

    [Header("Buttons")]
    public Button playButton;
    public Button optionsButton;
    public Button creditsButton;
    public Button quitButton;
    public Button backButtonOptions; // For options/credits
    public Button backButtonCredits; // For options/credits

    [Header("Game Settings")]
    public string gameSceneName = "Level1"; // First level scene name

    [Header("Audio")]
    public AudioClip buttonClickSound;
    public AudioClip menuMusic;

    [Header("Version Info")]
    public TextMeshProUGUI versionText;
    public string gameVersion = "v1.0";

    void Start()
    {
        // Show main menu, hide others
        ShowMainMenu();

        // Setup button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (optionsButton != null)
            optionsButton.onClick.AddListener(ShowOptions);

        if (creditsButton != null)
            creditsButton.onClick.AddListener(ShowCredits);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (backButtonOptions != null)
            backButtonOptions.onClick.AddListener(ShowMainMenu);

        if (backButtonCredits != null)
            backButtonCredits.onClick.AddListener(ShowMainMenu);

        // Display version
        if (versionText != null)
            versionText.text = gameVersion;

        // Play menu music
        if (AudioManager.instance != null && menuMusic != null)
        {
            AudioManager.instance.PlayMusic(menuMusic);
        }
    }

    public void PlayGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowOptions()
    {
        PlayButtonSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void ShowCredits()
    {
        PlayButtonSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        PlayButtonSound();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        PlayButtonSound();
        Debug.Log("Quitting game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    void PlayButtonSound()
    {
        if (AudioManager.instance != null && buttonClickSound != null)
        {
            AudioManager.instance.PlaySFX(buttonClickSound);
        }
    }
}