using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

// TilemapTinter.cs - Attach to Tilemap GameObject to tint tiles when player steps on them
[RequireComponent(typeof(Tilemap))]
public class TilemapTinter : MonoBehaviour
{
    [Header("Tint Settings")]
    [Tooltip("Color to tint the tile when stepped on")]
    public Color tintColor = new Color(0.5f, 0.8f, 1f, 1f); // Light blue default

    [Tooltip("How long the tint stays at full intensity before fading out")]
    public float tintDuration = 0.5f;

    [Header("Fade Settings")]
    [Tooltip("Speed of fade in effect")]
    public float fadeInSpeed = 2f;

    [Tooltip("Speed of fade out effect")]
    public float fadeOutSpeed = 1.5f;

    [Header("Behavior Settings")]
    [Tooltip("If true, tile stays tinted until player leaves. If false, fades out after duration")]
    public bool stayTintedWhilePlayerOnTile = false;

    [Tooltip("If true, tiles can be re-tinted multiple times. If false, only tints once")]
    public bool allowMultipleTints = true;

    [Tooltip("Cooldown time before a tile can be tinted again")]
    public float tintCooldown = 0.5f;

    [Header("Player Detection")]
    [Tooltip("How often to check for player position (in seconds). Lower = more accurate but more CPU intensive")]
    public float checkInterval = 0.1f;

    [Tooltip("Offset from player position to check tiles (useful if player's pivot is not at feet)")]
    public Vector2 playerFootOffset = new Vector2(0f, -0.5f);

    private Tilemap tilemap;
    private Dictionary<Vector3Int, TintedTileInfo> tintedTiles = new Dictionary<Vector3Int, TintedTileInfo>();
    private Dictionary<Vector3Int, Coroutine> activeFades = new Dictionary<Vector3Int, Coroutine>();
    private float checkTimer = 0f;
    private GameObject[] players;

    private class TintedTileInfo
    {
        public float lastTintTime;
        public bool hasBeenTinted;
        public bool isPlayerOnTile;
    }

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();

        if (tilemap == null)
        {
            Debug.LogError("TilemapTinter requires a Tilemap component!");
            enabled = false;
        }
    }

    void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckPlayerPositions();
        }
    }

    void CheckPlayerPositions()
    {
        // Find all active players
        players = GameObject.FindGameObjectsWithTag("Player");

        // Mark all tiles as not having a player on them this frame
        List<Vector3Int> tilesToCheck = new List<Vector3Int>(tintedTiles.Keys);
        foreach (var tilePos in tilesToCheck)
        {
            if (tintedTiles.ContainsKey(tilePos))
            {
                tintedTiles[tilePos].isPlayerOnTile = false;
            }
        }

        // Check each player
        foreach (GameObject player in players)
        {
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc == null || !pc.IsActive() || pc.IsDead())
                continue;

            // Get player's foot position
            Vector3 playerPos = player.transform.position;
            Vector3 footPos = playerPos + (Vector3)playerFootOffset;

            // Convert world position to tile position
            Vector3Int tilePos = tilemap.WorldToCell(footPos);

            // Check if there's a tile at this position
            if (tilemap.HasTile(tilePos))
            {
                TintTile(tilePos);
            }
        }

        // Check if any player left a tile
        foreach (var kvp in tintedTiles)
        {
            Vector3Int tilePos = kvp.Key;
            TintedTileInfo info = kvp.Value;

            if (!info.isPlayerOnTile && stayTintedWhilePlayerOnTile)
            {
                // Player left the tile, fade it out
                StartTileFadeOut(tilePos);
            }
        }
    }

    void TintTile(Vector3Int tilePos)
    {
        // Get or create tile info
        if (!tintedTiles.ContainsKey(tilePos))
        {
            tintedTiles[tilePos] = new TintedTileInfo
            {
                lastTintTime = 0f,
                hasBeenTinted = false,
                isPlayerOnTile = true
            };
        }
        else
        {
            tintedTiles[tilePos].isPlayerOnTile = true;
        }

        TintedTileInfo info = tintedTiles[tilePos];

        // Check if we can tint
        if (!allowMultipleTints && info.hasBeenTinted)
            return;

        if (Time.time - info.lastTintTime < tintCooldown)
            return;

        // Start tinting
        if (activeFades.ContainsKey(tilePos))
        {
            StopCoroutine(activeFades[tilePos]);
            activeFades.Remove(tilePos);
        }

        Coroutine fadeCoroutine = StartCoroutine(TintFadeSequence(tilePos));
        activeFades[tilePos] = fadeCoroutine;

        info.lastTintTime = Time.time;
        info.hasBeenTinted = true;
    }

    void StartTileFadeOut(Vector3Int tilePos)
    {
        if (activeFades.ContainsKey(tilePos))
        {
            StopCoroutine(activeFades[tilePos]);
        }

        Coroutine fadeCoroutine = StartCoroutine(FadeOut(tilePos));
        activeFades[tilePos] = fadeCoroutine;
    }

    IEnumerator TintFadeSequence(Vector3Int tilePos)
    {
        // Enable color modification for this tile
        tilemap.SetTileFlags(tilePos, TileFlags.None);

        // Fade in to tint color
        yield return StartCoroutine(FadeIn(tilePos));

        // If we should stay tinted while player is on tile, wait
        if (stayTintedWhilePlayerOnTile)
        {
            // The CheckPlayerPositions will handle fading out when player leaves
            if (activeFades.ContainsKey(tilePos))
                activeFades.Remove(tilePos);
            yield break;
        }

        // Otherwise, wait for duration then fade out
        yield return new WaitForSeconds(tintDuration);

        // Fade out back to original
        yield return StartCoroutine(FadeOut(tilePos));

        if (activeFades.ContainsKey(tilePos))
            activeFades.Remove(tilePos);
    }

    IEnumerator FadeIn(Vector3Int tilePos)
    {
        float elapsed = 0f;
        Color startColor = tilemap.GetColor(tilePos);
        Color targetColor = tintColor;

        while (elapsed < 1f / fadeInSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeInSpeed);

            // Smooth fade using ease-out
            t = 1f - Mathf.Pow(1f - t, 2f);

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            tilemap.SetColor(tilePos, currentColor);

            yield return null;
        }

        tilemap.SetColor(tilePos, targetColor);
    }

    IEnumerator FadeOut(Vector3Int tilePos)
    {
        float elapsed = 0f;
        Color startColor = tilemap.GetColor(tilePos);
        Color targetColor = Color.white; // Default tile color

        while (elapsed < 1f / fadeOutSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeOutSpeed);

            // Smooth fade using ease-in
            t = Mathf.Pow(t, 2f);

            Color currentColor = Color.Lerp(startColor, targetColor, t);
            tilemap.SetColor(tilePos, currentColor);

            yield return null;
        }

        tilemap.SetColor(tilePos, targetColor);

        // Reset tile flags
        tilemap.SetTileFlags(tilePos, TileFlags.LockColor);

        if (activeFades.ContainsKey(tilePos))
            activeFades.Remove(tilePos);
    }

    /// <summary>
    /// Reset all tinted tiles back to their original colors
    /// </summary>
    public void ResetAllTiles()
    {
        // Stop all active fades
        foreach (var kvp in activeFades)
        {
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);
        }
        activeFades.Clear();

        // Reset all tiles
        foreach (var tilePos in tintedTiles.Keys)
        {
            tilemap.SetColor(tilePos, Color.white);
            tilemap.SetTileFlags(tilePos, TileFlags.LockColor);
        }

        tintedTiles.Clear();
    }

    /// <summary>
    /// Manually tint a specific tile position
    /// </summary>
    public void TintTileAt(Vector3Int tilePos)
    {
        if (tilemap.HasTile(tilePos))
        {
            TintTile(tilePos);
        }
    }

    /// <summary>
    /// Manually tint a tile at world position
    /// </summary>
    public void TintTileAtWorldPosition(Vector3 worldPos)
    {
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        TintTileAt(tilePos);
    }

    void OnDrawGizmosSelected()
    {
        // Draw the player foot offset area
        GameObject[] debugPlayers = GameObject.FindGameObjectsWithTag("Player");
        Gizmos.color = tintColor;

        foreach (GameObject player in debugPlayers)
        {
            if (player != null)
            {
                Vector3 footPos = player.transform.position + (Vector3)playerFootOffset;
                Gizmos.DrawWireSphere(footPos, 0.2f);
            }
        }
    }

    void OnDisable()
    {
        // Clean up when disabled
        ResetAllTiles();
    }
}
