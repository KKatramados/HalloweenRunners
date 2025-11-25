using UnityEngine;
using TMPro;

// DamageTextManager.cs - Singleton manager for spawning damage float texts
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance;

    [Header("Prefab Settings")]
    public GameObject damageTextPrefab;

    [Header("Critical Hit Settings")]
    public float criticalHitChance = 0.15f; // 15% chance for critical hit

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create prefab at runtime if not assigned
        if (damageTextPrefab == null)
        {
            CreateDamageTextPrefab();
        }
    }

    /// <summary>
    /// Creates a damage text prefab at runtime
    /// </summary>
    void CreateDamageTextPrefab()
    {
        // Create GameObject for prefab
        damageTextPrefab = new GameObject("DamageTextPrefab");

        // Add TextMeshPro component
        TextMeshPro tmp = damageTextPrefab.AddComponent<TextMeshPro>();
        tmp.fontSize = 4;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.sortingOrder = 100; // Render on top

        // Add DamageFloatText component
        damageTextPrefab.AddComponent<DamageFloatText>();

        // Don't set parent and keep it as a prefab reference
        damageTextPrefab.SetActive(false);

        Debug.Log("DamageTextManager: Created runtime damage text prefab");
    }

    /// <summary>
    /// Show damage text at world position
    /// </summary>
    public void ShowDamage(int damage, Vector3 worldPosition, bool forceCritical = false)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("DamageTextManager: No damage text prefab assigned!");
            return;
        }

        // Determine if this is a critical hit
        bool isCritical = forceCritical || Random.value < criticalHitChance;

        // Apply critical damage multiplier
        if (isCritical && !forceCritical)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
        }

        // Instantiate damage text
        GameObject damageTextObj = Instantiate(damageTextPrefab, worldPosition, Quaternion.identity);
        damageTextObj.SetActive(true);

        // Initialize the text
        DamageFloatText floatText = damageTextObj.GetComponent<DamageFloatText>();
        if (floatText != null)
        {
            floatText.Initialize(damage, isCritical, worldPosition);
        }
    }

    /// <summary>
    /// Show damage text with explicit critical flag
    /// </summary>
    public void ShowDamageWithCritical(int damage, Vector3 worldPosition, bool isCritical)
    {
        if (damageTextPrefab == null)
        {
            Debug.LogWarning("DamageTextManager: No damage text prefab assigned!");
            return;
        }

        // Apply critical damage multiplier if critical
        int displayDamage = isCritical ? Mathf.RoundToInt(damage * 1.5f) : damage;

        // Instantiate damage text
        GameObject damageTextObj = Instantiate(damageTextPrefab, worldPosition, Quaternion.identity);
        damageTextObj.SetActive(true);

        // Initialize the text
        DamageFloatText floatText = damageTextObj.GetComponent<DamageFloatText>();
        if (floatText != null)
        {
            floatText.Initialize(displayDamage, isCritical, worldPosition);
        }
    }

    /// <summary>
    /// Check if an attack should be critical
    /// </summary>
    public bool RollForCritical()
    {
        return Random.value < criticalHitChance;
    }
}
