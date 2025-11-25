using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// GameManager.cs - Create empty GameObject and attach this
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI References")]
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI highScoreText;
    public Button restartButton;
    public TextMeshProUGUI player2PromptText;

    [Header("Player 1 UI")]
    public Image[] player1HealthIcons;
    public TextMeshProUGUI player1RedCandyText;
    public TextMeshProUGUI player1BlueCandyText;
    public TextMeshProUGUI player1GreenCandyText;
    public Image player1RedCandyIcon;
    public Image player1BlueCandyIcon;
    public Image player1GreenCandyIcon;

    [Header("Player 2 UI")]
    public Image[] player2HealthIcons;
    public TextMeshProUGUI player2RedCandyText;
    public TextMeshProUGUI player2BlueCandyText;
    public TextMeshProUGUI player2GreenCandyText;
    public Image player2RedCandyIcon;
    public Image player2BlueCandyIcon;
    public Image player2GreenCandyIcon;

    [Header("Respawn UI")]
    public TextMeshProUGUI player1RespawnPrompt;
    public TextMeshProUGUI player2RespawnPrompt;

    private bool isGameOver = false;
    private int alivePlayers = 0;

    private int player1Score = 0;
    private int player2Score = 0;
    private int deadPlayers = 0;
    private int totalPlayers = 1;
    private bool player2HasJoined = false;

    // Candy counts per player
    private int p1RedCandies = 0;
    private int p1BlueCandies = 0;
    private int p1GreenCandies = 0;
    private int p2RedCandies = 0;
    private int p2BlueCandies = 0;
    private int p2GreenCandies = 0;


    void Awake()
    {
        // Ensure singleton and proper initialization
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // CRITICAL: Ensure time is running
        Time.timeScale = 1f;
    }

    void Start()
    {
        // CRITICAL: Reset time scale in case it was paused
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        // Hide Player 2 UI on mobile
        if (Application.isMobilePlatform)
        {
            SetPlayer2UIVisibility(false);

            if (player2PromptText != null)
            {
                player2PromptText.gameObject.SetActive(false);
            }
        }
        else
        {
            if (player2PromptText != null)
            {
                player2PromptText.text = "Press Button to Join (P2)";
            }

            SetPlayer2UIVisibility(false);
        }

        if (player1RespawnPrompt != null)
            player1RespawnPrompt.gameObject.SetActive(false);

        if (player2RespawnPrompt != null)
            player2RespawnPrompt.gameObject.SetActive(false);

        UpdateUI();
    }

    public void ShowRespawnPrompt(int playerNumber, bool show)
    {
        if (playerNumber == 1 && player1RespawnPrompt != null)
        {
            player1RespawnPrompt.gameObject.SetActive(show);
            if (show)
            {
                player1RespawnPrompt.text = "Press SPACE to Respawn (P1)";
            }
        }
        else if (playerNumber == 2 && player2RespawnPrompt != null)
        {
            player2RespawnPrompt.gameObject.SetActive(show);
            if (show)
            {
                player2RespawnPrompt.text = "Press BUTTON to Respawn (P2)";
            }
        }
    }

    public void PlayerDied(int playerNumber)
    {
        if (isGameOver) return;

        deadPlayers++;

        // Check if all players are dead
        int totalAlivePlayers = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (var p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            if (pc != null && pc.IsActive() && !pc.IsDead())
            {
                totalAlivePlayers++;
            }
        }

        // Only game over if ALL players are dead
        if (totalAlivePlayers <= 0)
        {
            GameOver();
        }
        else
        {
            Debug.Log($"Player {playerNumber} died. {totalAlivePlayers} player(s) still alive.");
        }
    }


    public void Player2Joined()
    {
        if (player2HasJoined) return;

        player2HasJoined = true;
        totalPlayers = 2;

        if (player2PromptText != null)
        {
            player2PromptText.gameObject.SetActive(false);
        }

        SetPlayer2UIVisibility(true);
        UpdateUI();
        Debug.Log("Player 2 has joined!");
    }

    void SetPlayer2UIVisibility(bool visible)
    {
        if (player2ScoreText != null)
            player2ScoreText.gameObject.SetActive(visible);

        foreach (var icon in player2HealthIcons)
        {
            if (icon != null) icon.gameObject.SetActive(visible);
        }

        if (player2RedCandyText != null) player2RedCandyText.gameObject.SetActive(visible);
        if (player2BlueCandyText != null) player2BlueCandyText.gameObject.SetActive(visible);
        if (player2GreenCandyText != null) player2GreenCandyText.gameObject.SetActive(visible);
        if (player2RedCandyIcon != null) player2RedCandyIcon.gameObject.SetActive(visible);
        if (player2BlueCandyIcon != null) player2BlueCandyIcon.gameObject.SetActive(visible);
        if (player2GreenCandyIcon != null) player2GreenCandyIcon.gameObject.SetActive(visible);
    }

    public void AddCandy(int playerNumber, CandyCollectible.CandyType candyType)
    {
        if (isGameOver) return;

        if (playerNumber == 1)
        {
            switch (candyType)
            {
                case CandyCollectible.CandyType.Red:
                    p1RedCandies++;
                    break;
                case CandyCollectible.CandyType.Blue:
                    p1BlueCandies++;
                    break;
                case CandyCollectible.CandyType.Green:
                    p1GreenCandies++;
                    break;
            }
        }
        else
        {
            switch (candyType)
            {
                case CandyCollectible.CandyType.Red:
                    p2RedCandies++;
                    break;
                case CandyCollectible.CandyType.Blue:
                    p2BlueCandies++;
                    break;
                case CandyCollectible.CandyType.Green:
                    p2GreenCandies++;
                    break;
            }
        }

        UpdateUI();
    }

    public void UpdatePlayerHealth(int playerNumber, int health)
    {
        Image[] healthIcons = playerNumber == 1 ? player1HealthIcons : player2HealthIcons;

        for (int i = 0; i < healthIcons.Length; i++)
        {
            if (healthIcons[i] != null)
            {
                healthIcons[i].enabled = i < health;
            }
        }
    }

    public void AddScore(int points, int playerNumber)
    {
        if (isGameOver) return;

        if (playerNumber == 1)
            player1Score += points;
        else
            player2Score += points;

        UpdateUI();
    }

    public void AddScoreToAll(int points)
    {
        if (isGameOver) return;

        player1Score += points;
        if (player2HasJoined)
            player2Score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (player1ScoreText != null)
            player1ScoreText.text = "P1: " + player1Score;

        if (player2ScoreText != null && player2HasJoined)
            player2ScoreText.text = "P2: " + player2Score;

        // Update candy counts for Player 1
        if (player1RedCandyText != null) player1RedCandyText.text = p1RedCandies.ToString();
        if (player1BlueCandyText != null) player1BlueCandyText.text = p1BlueCandies.ToString();
        if (player1GreenCandyText != null) player1GreenCandyText.text = p1GreenCandies.ToString();

        // Update candy counts for Player 2
        if (player2HasJoined)
        {
            if (player2RedCandyText != null) player2RedCandyText.text = p2RedCandies.ToString();
            if (player2BlueCandyText != null) player2BlueCandyText.text = p2BlueCandies.ToString();
            if (player2GreenCandyText != null) player2GreenCandyText.text = p2GreenCandies.ToString();
        }
    }


    void GameOver()
    {
        if (isGameOver) return; // Prevent multiple calls

        isGameOver = true;

        int finalScore = player1Score + player2Score;
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        // AUDIO: Play game over music
        if (AudioManager.instance != null)
            AudioManager.instance.PlayMusic(AudioManager.instance.gameOverMusic);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverScoreText != null)
                gameOverScoreText.text = "Score: " + finalScore;

            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore;
        }

        // Auto restart after 3 seconds
        Invoke("RestartGame", 3f);
    }

    public void RestartGame()
    {
        StartCoroutine(RestartGameCoroutine());
    }

    System.Collections.IEnumerator RestartGameCoroutine()
    {
        // Cancel any pending invokes
        CancelInvoke();

        // Ensure time scale is reset
        Time.timeScale = 1f;

        // Fade out or show loading screen here
        yield return new WaitForSeconds(0.5f);

        // Destroy AudioManager
        if (AudioManager.instance != null)
        {
            Destroy(AudioManager.instance.gameObject);
            AudioManager.instance = null;
        }

        // Clear singleton
        if (instance == this)
        {
            instance = null;
        }

        // Small delay for cleanup
        yield return null;

        // Reload scene
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void PlayerRespawned(int playerNumber)
{
    if (isGameOver) return;
    
    deadPlayers--;
    if (deadPlayers < 0) deadPlayers = 0;
    
    Debug.Log($"Player {playerNumber} respawned!");
}

    public void BossDefeated()
    {
        if (isGameOver) return;

        isGameOver = true;

        // AUDIO: Victory music already played in boss Die() method
        Debug.Log("Boss Defeated! You Win!");
        Invoke("GameOver", 2f);
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
    void OnDestroy()
    {
        // Clear singleton reference when destroyed
        if (instance == this)
        {
            instance = null;
        }
    }
}