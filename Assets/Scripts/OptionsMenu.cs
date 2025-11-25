using UnityEngine;
using UnityEngine.UI;
using TMPro;

// OptionsMenu.cs - Handles options/settings
public class OptionsMenu : MonoBehaviour
{
    [Header("Volume Sliders")]
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Volume Labels")]
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI sfxVolumeText;

    [Header("Fullscreen")]
    public Toggle fullscreenToggle;

    void Start()
    {
        LoadSettings();

        // Setup listeners
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    void LoadSettings()
    {
        // Load saved settings
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVol;

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = sfxVol;

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = fullscreen;

        // Apply settings
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(musicVol);
            AudioManager.instance.SetSFXVolume(sfxVol);
        }

        Screen.fullScreen = fullscreen;

        UpdateVolumeLabels();
    }

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetMusicVolume(volume);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
        UpdateVolumeLabels();
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.instance != null)
        {
            AudioManager.instance.SetSFXVolume(volume);
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
        UpdateVolumeLabels();
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    void UpdateVolumeLabels()
    {
        if (musicVolumeText != null && musicVolumeSlider != null)
        {
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
        }

        if (sfxVolumeText != null && sfxVolumeSlider != null)
        {
            sfxVolumeText.text = Mathf.RoundToInt(sfxVolumeSlider.value * 100) + "%";
        }
    }
}