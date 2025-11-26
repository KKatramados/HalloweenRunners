using UnityEngine;
using System.Collections;

// TintableTile.cs - Attach to ground tiles that should tint when player steps on them
public class TintableTile : MonoBehaviour
{
    [Header("Tint Settings")]
    [Tooltip("Color to tint the tile when stepped on")]
    public Color tintColor = new Color(0.5f, 0.8f, 1f, 0.5f); // Light blue default

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

    [Tooltip("If true, tile can be re-tinted multiple times. If false, only tints once")]
    public bool allowMultipleTints = true;

    [Tooltip("Cooldown time before tile can be tinted again (only if allowMultipleTints is true)")]
    public float tintCooldown = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine fadeCoroutine;
    private bool isTinted = false;
    private bool hasBeenTinted = false;
    private float lastTintTime = 0f;
    private int playersOnTile = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"TintableTile on {gameObject.name} requires a SpriteRenderer component!");
            enabled = false;
            return;
        }

        originalColor = spriteRenderer.color;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersOnTile++;
            PlayerStepped();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playersOnTile--;
            if (playersOnTile <= 0)
            {
                playersOnTile = 0;
                PlayerLeft();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playersOnTile++;
            PlayerStepped();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playersOnTile--;
            if (playersOnTile <= 0)
            {
                playersOnTile = 0;
                PlayerLeft();
            }
        }
    }

    void PlayerStepped()
    {
        // Check if we can tint
        if (!allowMultipleTints && hasBeenTinted)
            return;

        if (Time.time - lastTintTime < tintCooldown)
            return;

        // Start tinting
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(TintFadeSequence());
        lastTintTime = Time.time;
        hasBeenTinted = true;
    }

    void PlayerLeft()
    {
        // If we're set to stay tinted while player is on tile, fade out when they leave
        if (stayTintedWhilePlayerOnTile && isTinted)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }

    IEnumerator TintFadeSequence()
    {
        isTinted = true;

        // Fade in to tint color
        yield return StartCoroutine(FadeIn());

        // If we should stay tinted while player is on tile, wait
        if (stayTintedWhilePlayerOnTile)
        {
            // Wait until player leaves (handled in PlayerLeft)
            yield break;
        }

        // Otherwise, wait for duration then fade out
        yield return new WaitForSeconds(tintDuration);

        // Fade out back to original
        yield return StartCoroutine(FadeOut());

        isTinted = false;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color targetColor = Color.Lerp(originalColor, tintColor, 1f);

        while (elapsed < 1f / fadeInSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeInSpeed);

            // Smooth fade using ease-out
            t = 1f - Mathf.Pow(1f - t, 2f);

            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        spriteRenderer.color = targetColor;
    }

    IEnumerator FadeOut()
    {
        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < 1f / fadeOutSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed * fadeOutSpeed);

            // Smooth fade using ease-in
            t = Mathf.Pow(t, 2f);

            spriteRenderer.color = Color.Lerp(startColor, originalColor, t);
            yield return null;
        }

        spriteRenderer.color = originalColor;
        isTinted = false;
    }

    // Public method to reset the tile (useful for level resets)
    public void ResetTile()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        spriteRenderer.color = originalColor;
        isTinted = false;
        hasBeenTinted = false;
        playersOnTile = 0;
    }

    // Public method to manually trigger tint (useful for effects)
    public void TriggerTint()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(TintFadeSequence());
    }

    void OnDrawGizmosSelected()
    {
        // Draw a colored box showing the tint area
        Gizmos.color = tintColor;

        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
