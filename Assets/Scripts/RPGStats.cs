using UnityEngine;
using System;

// RPGStats.cs - Attach to player GameObject alongside PlayerController
// Manages experience, leveling, and character stats
public class RPGStats : MonoBehaviour
{
    [Header("Level System")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;
    public float xpScalingFactor = 1.5f; // Each level requires 50% more XP

    [Header("Base Stats")]
    public int baseStrength = 5;
    public int baseDefense = 5;
    public int baseVitality = 5;
    public int baseAgility = 5;

    [Header("Current Stats")]
    public int strength = 5;      // Increases attack damage
    public int defense = 5;       // Reduces incoming damage
    public int vitality = 5;      // Increases max health
    public int agility = 5;       // Increases movement speed

    [Header("Stat Points")]
    public int availableStatPoints = 0;
    public int statPointsPerLevel = 3;

    [Header("Skill System")]
    public int availableSkillPoints = 0;
    public int skillPointsPerLevel = 1;

    [Header("Equipment Bonuses")]
    public int equipmentStrengthBonus = 0;
    public int equipmentDefenseBonus = 0;
    public int equipmentVitalityBonus = 0;
    public int equipmentAgilityBonus = 0;

    // Events
    public event Action OnLevelUp;
    public event Action<int> OnXPGained;
    public event Action<string, int> OnStatIncreased;

    private PlayerController playerController;
    private int playerNumber;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        playerNumber = playerController.playerNumber;

        // Initialize stats
        RecalculateStats();
        UpdatePlayerStats();
    }

    /// <summary>
    /// Add experience points and check for level up
    /// </summary>
    public void AddXP(int amount)
    {
        currentXP += amount;
        OnXPGained?.Invoke(amount);

        // Check for level up
        while (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        // Update UI
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.UpdateXPBar(playerNumber, currentXP, xpToNextLevel);
        }
    }

    /// <summary>
    /// Level up the player
    /// </summary>
    void LevelUp()
    {
        currentLevel++;
        currentXP -= xpToNextLevel;
        
        // Calculate next level XP requirement
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * xpScalingFactor);

        // Award stat and skill points
        availableStatPoints += statPointsPerLevel;
        availableSkillPoints += skillPointsPerLevel;

        // Increase base stats slightly
        baseStrength += 1;
        baseDefense += 1;
        baseVitality += 1;
        baseAgility += 1;

        // Recalculate and apply stats
        RecalculateStats();
        UpdatePlayerStats();

        // Heal player on level up
        if (playerController != null)
        {
            int maxHealth = GetMaxHealth();
            playerController.maxHealth = maxHealth;
            // Note: You'll need to expose currentHealth or add a Heal method in PlayerController
        }

        // Trigger event
        OnLevelUp?.Invoke();

        // Play level up effect
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        // Update UI
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.ShowLevelUpNotification(playerNumber, currentLevel);
            RPGUIManager.instance.UpdateStatsDisplay(playerNumber, this);
        }

        Debug.Log($"Player {playerNumber} reached level {currentLevel}!");
    }

    /// <summary>
    /// Increase a specific stat using available stat points
    /// </summary>
    public bool IncreaseStrength()
    {
        if (availableStatPoints <= 0) return false;
        
        baseStrength++;
        availableStatPoints--;
        RecalculateStats();
        UpdatePlayerStats();
        OnStatIncreased?.Invoke("Strength", baseStrength);
        
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.UpdateStatsDisplay(playerNumber, this);
        }
        
        return true;
    }

    public bool IncreaseDefense()
    {
        if (availableStatPoints <= 0) return false;
        
        baseDefense++;
        availableStatPoints--;
        RecalculateStats();
        UpdatePlayerStats();
        OnStatIncreased?.Invoke("Defense", baseDefense);
        
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.UpdateStatsDisplay(playerNumber, this);
        }
        
        return true;
    }

    public bool IncreaseVitality()
    {
        if (availableStatPoints <= 0) return false;
        
        baseVitality++;
        availableStatPoints--;
        RecalculateStats();
        UpdatePlayerStats();
        OnStatIncreased?.Invoke("Vitality", baseVitality);
        
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.UpdateStatsDisplay(playerNumber, this);
        }
        
        return true;
    }

    public bool IncreaseAgility()
    {
        if (availableStatPoints <= 0) return false;
        
        baseAgility++;
        availableStatPoints--;
        RecalculateStats();
        UpdatePlayerStats();
        OnStatIncreased?.Invoke("Agility", baseAgility);
        
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.UpdateStatsDisplay(playerNumber, this);
        }
        
        return true;
    }

    /// <summary>
    /// Recalculate all stats including equipment bonuses
    /// </summary>
    void RecalculateStats()
    {
        strength = baseStrength + equipmentStrengthBonus;
        defense = baseDefense + equipmentDefenseBonus;
        vitality = baseVitality + equipmentVitalityBonus;
        agility = baseAgility + equipmentAgilityBonus;
    }

    /// <summary>
    /// Apply stat bonuses to player controller
    /// </summary>
    void UpdatePlayerStats()
    {
        if (playerController == null) return;

        // Update max health based on vitality
        int newMaxHealth = GetMaxHealth();
        playerController.maxHealth = newMaxHealth;

        // Update movement speed based on agility
        float speedMultiplier = 1f + (agility * 0.02f); // 2% speed per agility point
        playerController.moveSpeed = 5f * speedMultiplier;

        // Update GameManager UI
        if (GameManager.instance != null)
        {
            GameManager.instance.UpdatePlayerHealth(playerNumber, playerController.GetHealth());
        }
    }

    /// <summary>
    /// Calculate damage based on strength stat
    /// </summary>
    public int GetAttackDamage(int baseDamage)
    {
        float damageMultiplier = 1f + (strength * 0.1f); // 10% damage per strength point
        return Mathf.RoundToInt(baseDamage * damageMultiplier);
    }

    /// <summary>
    /// Calculate damage reduction based on defense stat
    /// </summary>
    public int CalculateIncomingDamage(int incomingDamage)
    {
        float damageReduction = defense * 0.05f; // 5% reduction per defense point
        damageReduction = Mathf.Clamp(damageReduction, 0f, 0.75f); // Max 75% reduction
        
        int reducedDamage = Mathf.RoundToInt(incomingDamage * (1f - damageReduction));
        return Mathf.Max(1, reducedDamage); // Always deal at least 1 damage
    }

    /// <summary>
    /// Get max health based on vitality
    /// </summary>
    public int GetMaxHealth()
    {
        return 6 + (vitality * 2); // 6 base + 2 health per vitality point
    }

    /// <summary>
    /// Add equipment bonuses
    /// </summary>
    public void AddEquipmentBonus(EquipmentStats equipmentStats)
    {
        equipmentStrengthBonus += equipmentStats.strengthBonus;
        equipmentDefenseBonus += equipmentStats.defenseBonus;
        equipmentVitalityBonus += equipmentStats.vitalityBonus;
        equipmentAgilityBonus += equipmentStats.agilityBonus;
        
        RecalculateStats();
        UpdatePlayerStats();
    }

    /// <summary>
    /// Remove equipment bonuses
    /// </summary>
    public void RemoveEquipmentBonus(EquipmentStats equipmentStats)
    {
        equipmentStrengthBonus -= equipmentStats.strengthBonus;
        equipmentDefenseBonus -= equipmentStats.defenseBonus;
        equipmentVitalityBonus -= equipmentStats.vitalityBonus;
        equipmentAgilityBonus -= equipmentStats.agilityBonus;
        
        RecalculateStats();
        UpdatePlayerStats();
    }

    /// <summary>
    /// Save RPG stats to PlayerPrefs
    /// </summary>
    public void SaveStats()
    {
        string prefix = $"P{playerNumber}_";
        
        PlayerPrefs.SetInt(prefix + "Level", currentLevel);
        PlayerPrefs.SetInt(prefix + "XP", currentXP);
        PlayerPrefs.SetInt(prefix + "XPToNext", xpToNextLevel);
        
        PlayerPrefs.SetInt(prefix + "BaseStr", baseStrength);
        PlayerPrefs.SetInt(prefix + "BaseDef", baseDefense);
        PlayerPrefs.SetInt(prefix + "BaseVit", baseVitality);
        PlayerPrefs.SetInt(prefix + "BaseAgi", baseAgility);
        
        PlayerPrefs.SetInt(prefix + "StatPoints", availableStatPoints);
        PlayerPrefs.SetInt(prefix + "SkillPoints", availableSkillPoints);
        
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load RPG stats from PlayerPrefs
    /// </summary>
    public void LoadStats()
    {
        string prefix = $"P{playerNumber}_";
        
        currentLevel = PlayerPrefs.GetInt(prefix + "Level", 1);
        currentXP = PlayerPrefs.GetInt(prefix + "XP", 0);
        xpToNextLevel = PlayerPrefs.GetInt(prefix + "XPToNext", 100);
        
        baseStrength = PlayerPrefs.GetInt(prefix + "BaseStr", 5);
        baseDefense = PlayerPrefs.GetInt(prefix + "BaseDef", 5);
        baseVitality = PlayerPrefs.GetInt(prefix + "BaseVit", 5);
        baseAgility = PlayerPrefs.GetInt(prefix + "BaseAgi", 5);
        
        availableStatPoints = PlayerPrefs.GetInt(prefix + "StatPoints", 0);
        availableSkillPoints = PlayerPrefs.GetInt(prefix + "SkillPoints", 0);
        
        RecalculateStats();
        UpdatePlayerStats();
    }
}

/// <summary>
/// Equipment stats data structure
/// </summary>
[System.Serializable]
public class EquipmentStats
{
    public int strengthBonus;
    public int defenseBonus;
    public int vitalityBonus;
    public int agilityBonus;
}
