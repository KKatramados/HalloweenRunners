using UnityEngine;
using TMPro;

// DamageFloatText.cs - Handles individual damage text animation
public class DamageFloatText : MonoBehaviour
{
    [Header("Animation Settings")]
    public float floatSpeed = 1.5f;
    public float lifetime = 1.5f;
    public float fadeSpeed = 1.0f;
    public float randomXOffset = 0.3f;
    public float scaleMultiplier = 1.2f;

    private TextMeshPro textMesh;
    private Color originalColor;
    private Vector3 velocity;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh == null)
        {
            Debug.LogError("DamageFloatText requires a TextMeshPro component!");
        }
    }

    public void Initialize(int damage, bool isCritical, Vector3 worldPosition)
    {
        if (textMesh == null) return;

        // Set damage text
        textMesh.text = damage.ToString();

        // Determine color based on damage amount and critical status
        if (isCritical)
        {
            // Critical hits - bright yellow/gold
            textMesh.color = new Color(1f, 0.84f, 0f); // Gold
            textMesh.fontSize = 5f;
            scaleMultiplier = 1.5f;
        }
        else
        {
            // Color gradient based on damage amount
            if (damage <= 2)
            {
                // Low damage - white/light grey
                textMesh.color = new Color(0.9f, 0.9f, 0.9f);
                textMesh.fontSize = 3f;
            }
            else if (damage <= 5)
            {
                // Medium damage - orange
                textMesh.color = new Color(1f, 0.65f, 0f);
                textMesh.fontSize = 4f;
            }
            else
            {
                // High damage - red
                textMesh.color = new Color(1f, 0.2f, 0.2f);
                textMesh.fontSize = 4.5f;
            }
        }

        originalColor = textMesh.color;

        // Set position with random offset
        transform.position = worldPosition + new Vector3(
            Random.Range(-randomXOffset, randomXOffset),
            0.5f,
            0
        );

        // Set velocity with slight random variation
        velocity = new Vector3(
            Random.Range(-0.2f, 0.2f),
            floatSpeed,
            0
        );

        // Scale animation for critical hits
        if (isCritical)
        {
            StartCoroutine(CriticalScalePulse());
        }

        // Auto-destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (textMesh == null) return;

        timer += Time.deltaTime;

        // Move upward
        transform.position += velocity * Time.deltaTime;

        // Slow down over time
        velocity.y *= 0.95f;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Scale up slightly over time
        float scale = Mathf.Lerp(1f, scaleMultiplier, timer / (lifetime * 0.5f));
        transform.localScale = Vector3.one * scale;
    }

    System.Collections.IEnumerator CriticalScalePulse()
    {
        float pulseTime = 0.2f;
        float elapsed = 0f;

        while (elapsed < pulseTime)
        {
            elapsed += Time.deltaTime;
            float scale = Mathf.Lerp(0.5f, 1.5f, elapsed / pulseTime);
            transform.localScale = Vector3.one * scale;
            yield return null;
        }
    }
}
