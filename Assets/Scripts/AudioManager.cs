using UnityEngine;

// AudioManager.cs - Fixed version with proper null checks
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainMenuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip bossMusic;
    public AudioClip victoryMusic;
    public AudioClip gameOverMusic;

    [Header("Player SFX")]
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip healSound;

    [Header("Enemy SFX")]
    public AudioClip enemyHitSound;
    public AudioClip enemyDeathSound;
    public AudioClip bossHitSound;
    public AudioClip bossRoarSound;
    public AudioClip bossDeathSound;

    [Header("Collectibles SFX")]
    public AudioClip coinSound;
    public AudioClip candySound;
    public AudioClip powerUpSound;

    [Header("Environment SFX")]
    public AudioClip doorOpenSound;
    public AudioClip footstepSound;
    public AudioClip projectileSound;

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    private bool isQuitting = false;

    void Awake()
    {
        // Singleton pattern with proper handling
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup audio sources
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.loop = true;
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // Play default music
        PlayMusic(level1Music);
    }

    // Music Methods with null checks
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null || isQuitting) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null && !isQuitting)
        {
            musicSource.Stop();
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && !isQuitting)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !isQuitting)
        {
            musicSource.UnPause();
        }
    }

    // SFX Methods with null checks
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null || isQuitting) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null || sfxSource == null || isQuitting) return;
        sfxSource.PlayOneShot(clip, volume);
    }

    // Specific sound effect methods with null checks
    public void PlayJump()
    {
        if (jumpSound != null && sfxSource != null && !isQuitting)
            PlaySFX(jumpSound);
    }

    public void PlayAttack()
    {
        if (attackSound != null && sfxSource != null && !isQuitting)
            PlaySFX(attackSound);
    }

    public void PlayHurt()
    {
        if (hurtSound != null && sfxSource != null && !isQuitting)
            PlaySFX(hurtSound);
    }

    public void PlayDeath()
    {
        if (deathSound != null && sfxSource != null && !isQuitting)
            PlaySFX(deathSound);
    }

    public void PlayHeal()
    {
        if (healSound != null && sfxSource != null && !isQuitting)
            PlaySFX(healSound);
    }

    public void PlayEnemyHit()
    {
        if (enemyHitSound != null && sfxSource != null && !isQuitting)
            PlaySFX(enemyHitSound);
    }

    public void PlayEnemyDeath()
    {
        if (enemyDeathSound != null && sfxSource != null && !isQuitting)
            PlaySFX(enemyDeathSound);
    }

    public void PlayBossHit()
    {
        if (bossHitSound != null && sfxSource != null && !isQuitting)
            PlaySFX(bossHitSound);
    }

    public void PlayBossRoar()
    {
        if (bossRoarSound != null && sfxSource != null && !isQuitting)
            PlaySFX(bossRoarSound);
    }

    public void PlayBossDeath()
    {
        if (bossDeathSound != null && sfxSource != null && !isQuitting)
            PlaySFX(bossDeathSound);
    }

    public void PlayCoin()
    {
        if (coinSound != null && sfxSource != null && !isQuitting)
            PlaySFX(coinSound);
    }

    public void PlayCandy()
    {
        if (candySound != null && sfxSource != null && !isQuitting)
            PlaySFX(candySound);
    }

    public void PlayPowerUp()
    {
        if (powerUpSound != null && sfxSource != null && !isQuitting)
            PlaySFX(powerUpSound);
    }

    public void PlayDoorOpen()
    {
        if (doorOpenSound != null && sfxSource != null && !isQuitting)
            PlaySFX(doorOpenSound);
    }

    public void PlayFootstep()
    {
        if (footstepSound != null && sfxSource != null && !isQuitting)
            PlaySFX(footstepSound, 0.5f);
    }

    public void PlayProjectile()
    {
        if (projectileSound != null && sfxSource != null && !isQuitting)
            PlaySFX(projectileSound);
    }

    // Volume control with null checks
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null && !isQuitting)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null && !isQuitting)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}