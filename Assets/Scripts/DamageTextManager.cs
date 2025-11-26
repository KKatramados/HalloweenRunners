using UnityEngine;
using TMPro;

// DamageTextManager.cs - Singleton manager for spawning damage float texts
public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager instance;

    [Header("Prefab Settings")]
    public GameObject damageTextPrefab;

    [Header("Rendering Settings")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 100;

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
        Debug.Log("DamageTextManager: Attempting to create runtime damage text prefab...");

        try
        {
            // Create GameObject for prefab
            damageTextPrefab = new GameObject("DamageTextPrefab");

            // Add TextMeshPro component (world space)
            TextMeshPro tmp = damageTextPrefab.AddComponent<TextMeshPro>();

            if (tmp == null)
            {
                Debug.LogError("DamageTextManager: Failed to add TextMeshPro component! Make sure TextMesh Pro is imported: Window > TextMeshPro > Import TMP Essential Resources");
                Destroy(damageTextPrefab);
                damageTextPrefab = null;
                return;
            }

            // Configure TextMeshPro
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;

            // Set sorting layer and order
            var renderer = tmp.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder;
            }

            tmp.color = Color.white;
            tmp.text = "0";

            // Add DamageFloatText component
            DamageFloatText floatText = damageTextPrefab.AddComponent<DamageFloatText>();

            if (floatText == null)
            {
                Debug.LogError("DamageTextManager: Failed to add DamageFloatText component!");
                Destroy(damageTextPrefab);
                damageTextPrefab = null;
                return;
            }

            // Don't set parent and keep it as a prefab reference
            damageTextPrefab.SetActive(false);

            Debug.Log("DamageTextManager: Successfully created runtime damage text prefab!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("DamageTextManager: Exception while creating prefab: " + e.Message);
            if (damageTextPrefab != null)
            {
                Destroy(damageTextPrefab);
                damageTextPrefab = null;
            }
        }
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
