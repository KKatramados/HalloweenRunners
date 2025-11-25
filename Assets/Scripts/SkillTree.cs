using UnityEngine;
using System.Collections.Generic;

// SkillTree.cs - Attach to player GameObject alongside RPGStats
// Manages unlockable skills and abilities
public class SkillTree : MonoBehaviour
{
    [Header("Skill System")]
    public List<Skill> unlockedSkills = new List<Skill>();
    public List<Skill> availableSkills = new List<Skill>();

    [Header("Active Skills")]
    public Skill equippedSkill1;
    public Skill equippedSkill2;
    public Skill equippedSkill3;

    [Header("Skill Cooldowns")]
    private float skill1Cooldown = 0f;
    private float skill2Cooldown = 0f;
    private float skill3Cooldown = 0f;

    private RPGStats rpgStats;
    private PlayerController playerController;
    private int playerNumber;

    void Start()
    {
        rpgStats = GetComponent<RPGStats>();
        playerController = GetComponent<PlayerController>();
        playerNumber = playerController.playerNumber;

        // Initialize default skills
        InitializeSkills();
    }

    void Update()
    {
        // Update cooldowns
        if (skill1Cooldown > 0) skill1Cooldown -= Time.deltaTime;
        if (skill2Cooldown > 0) skill2Cooldown -= Time.deltaTime;
        if (skill3Cooldown > 0) skill3Cooldown -= Time.deltaTime;

        // Check for skill input
        if (Input.GetKeyDown(KeyCode.Alpha1) && playerNumber == 1)
        {
            UseSkill(equippedSkill1, ref skill1Cooldown);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && playerNumber == 1)
        {
            UseSkill(equippedSkill2, ref skill2Cooldown);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && playerNumber == 1)
        {
            UseSkill(equippedSkill3, ref skill3Cooldown);
        }
    }

    void InitializeSkills()
    {
        // Define all available skills in the game
        availableSkills = new List<Skill>
        {
            // Combat Skills
            new Skill
            {
                skillName = "Power Strike",
                skillType = SkillType.Active,
                description = "Deal 2x damage on next attack",
                requiredLevel = 3,
                skillPointCost = 1,
                cooldown = 10f,
                icon = null // Assign in Inspector
            },
            new Skill
            {
                skillName = "Rapid Strikes",
                skillType = SkillType.Active,
                description = "Attack 3 times rapidly",
                requiredLevel = 5,
                skillPointCost = 2,
                cooldown = 15f,
                icon = null
            },
            new Skill
            {
                skillName = "Whirlwind",
                skillType = SkillType.Active,
                description = "Spin attack hitting all nearby enemies",
                requiredLevel = 8,
                skillPointCost = 2,
                cooldown = 20f,
                icon = null
            },

            // Defensive Skills
            new Skill
            {
                skillName = "Iron Skin",
                skillType = SkillType.Passive,
                description = "+10 Defense permanently",
                requiredLevel = 2,
                skillPointCost = 1,
                cooldown = 0f,
                icon = null
            },
            new Skill
            {
                skillName = "Dodge Roll",
                skillType = SkillType.Active,
                description = "Quick dash with brief invincibility",
                requiredLevel = 4,
                skillPointCost = 1,
                cooldown = 8f,
                icon = null
            },
            new Skill
            {
                skillName = "Shield Block",
                skillType = SkillType.Active,
                description = "Block incoming damage for 3 seconds",
                requiredLevel = 6,
                skillPointCost = 2,
                cooldown = 12f,
                icon = null
            },

            // Movement Skills
            new Skill
            {
                skillName = "Sprint",
                skillType = SkillType.Passive,
                description = "+20% movement speed permanently",
                requiredLevel = 3,
                skillPointCost = 1,
                cooldown = 0f,
                icon = null
            },
            new Skill
            {
                skillName = "Double Jump Mastery",
                skillType = SkillType.Passive,
                description = "Gain a 3rd jump",
                requiredLevel = 5,
                skillPointCost = 2,
                cooldown = 0f,
                icon = null
            },
            new Skill
            {
                skillName = "Dash",
                skillType = SkillType.Active,
                description = "Quick horizontal dash",
                requiredLevel = 4,
                skillPointCost = 1,
                cooldown = 5f,
                icon = null
            },

            // Utility Skills
            new Skill
            {
                skillName = "Life Steal",
                skillType = SkillType.Passive,
                description = "Heal 1 HP every 5 enemy kills",
                requiredLevel = 7,
                skillPointCost = 2,
                cooldown = 0f,
                icon = null
            },
            new Skill
            {
                skillName = "Treasure Hunter",
                skillType = SkillType.Passive,
                description = "Coins are worth 2x points",
                requiredLevel = 2,
                skillPointCost = 1,
                cooldown = 0f,
                icon = null
            },
            new Skill
            {
                skillName = "Heal",
                skillType = SkillType.Active,
                description = "Restore 4 HP immediately",
                requiredLevel = 6,
                skillPointCost = 2,
                cooldown = 30f,
                icon = null
            }
        };
    }

    /// <summary>
    /// Attempt to unlock a skill
    /// </summary>
    public bool UnlockSkill(Skill skill)
    {
        // Check requirements
        if (rpgStats.currentLevel < skill.requiredLevel)
        {
            Debug.Log($"Level {skill.requiredLevel} required to unlock {skill.skillName}");
            return false;
        }

        if (rpgStats.availableSkillPoints < skill.skillPointCost)
        {
            Debug.Log($"Not enough skill points to unlock {skill.skillName}");
            return false;
        }

        if (unlockedSkills.Contains(skill))
        {
            Debug.Log($"{skill.skillName} already unlocked");
            return false;
        }

        // Unlock the skill
        rpgStats.availableSkillPoints -= skill.skillPointCost;
        unlockedSkills.Add(skill);

        // Apply passive skill effects immediately
        if (skill.skillType == SkillType.Passive)
        {
            ApplyPassiveSkill(skill);
        }

        Debug.Log($"Unlocked {skill.skillName}!");
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        return true;
    }

    /// <summary>
    /// Apply passive skill effects
    /// </summary>
    void ApplyPassiveSkill(Skill skill)
    {
        switch (skill.skillName)
        {
            case "Iron Skin":
                rpgStats.baseDefense += 10;
                break;

            case "Sprint":
                rpgStats.baseAgility += 5;
                break;

            case "Double Jump Mastery":
                playerController.maxJumps = 3;
                break;

            case "Treasure Hunter":
                // This will be checked in PlayerController coin collection
                break;

            case "Life Steal":
                // This will be checked in enemy death
                break;
        }
    }

    /// <summary>
    /// Use an active skill
    /// </summary>
    void UseSkill(Skill skill, ref float cooldownTimer)
    {
        if (skill == null) return;
        
        if (!unlockedSkills.Contains(skill))
        {
            Debug.Log($"{skill.skillName} not unlocked");
            return;
        }

        if (cooldownTimer > 0)
        {
            Debug.Log($"{skill.skillName} on cooldown: {cooldownTimer:F1}s");
            return;
        }

        // Execute skill
        ExecuteActiveSkill(skill);
        
        // Start cooldown
        cooldownTimer = skill.cooldown;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayAttack();
        }
    }

    /// <summary>
    /// Execute active skill effects
    /// </summary>
    void ExecuteActiveSkill(Skill skill)
    {
        switch (skill.skillName)
        {
            case "Power Strike":
                StartCoroutine(PowerStrikeEffect());
                break;

            case "Rapid Strikes":
                StartCoroutine(RapidStrikesEffect());
                break;

            case "Whirlwind":
                StartCoroutine(WhirlwindEffect());
                break;

            case "Dodge Roll":
                StartCoroutine(DodgeRollEffect());
                break;

            case "Shield Block":
                StartCoroutine(ShieldBlockEffect());
                break;

            case "Dash":
                StartCoroutine(DashEffect());
                break;

            case "Heal":
                HealEffect();
                break;
        }

        Debug.Log($"Used {skill.skillName}!");
    }

    // Skill Effect Coroutines
    System.Collections.IEnumerator PowerStrikeEffect()
    {
        // Next attack deals 2x damage
        // You'll need to add a damage multiplier in Projectile.cs
        Debug.Log("Next attack deals double damage!");
        yield return null;
    }

    System.Collections.IEnumerator RapidStrikesEffect()
    {
        // Fire 3 projectiles rapidly
        for (int i = 0; i < 3; i++)
        {
            if (playerController != null)
            {
                // Trigger attack through PlayerController
                // You may need to expose the Attack() method
                Debug.Log($"Rapid Strike {i + 1}");
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    System.Collections.IEnumerator WhirlwindEffect()
    {
        // Damage all nearby enemies
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, 2f);
        int damage = rpgStats.GetAttackDamage(2);

        foreach (var enemy in enemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }

            SkeletonBoss boss = enemy.GetComponent<SkeletonBoss>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
            }
        }

        Debug.Log($"Whirlwind hit {enemies.Length} enemies!");
        yield return null;
    }

    System.Collections.IEnumerator DodgeRollEffect()
    {
        // Make player invincible and dash
        Debug.Log("Dodge roll activated!");
        
        // Apply dash movement
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * 15f, 0);
        }

        // Brief invincibility (you'll need to add this to PlayerController)
        yield return new WaitForSeconds(0.3f);
        
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    System.Collections.IEnumerator ShieldBlockEffect()
    {
        Debug.Log("Shield block active for 3 seconds!");
        // Reduce incoming damage to 0 for duration
        // You'll need to add a shielded flag in PlayerController
        yield return new WaitForSeconds(3f);
        Debug.Log("Shield block ended");
    }

    System.Collections.IEnumerator DashEffect()
    {
        Debug.Log("Dash!");
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float direction = transform.localScale.x > 0 ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * 20f, rb.linearVelocity.y);
        }

        yield return new WaitForSeconds(0.15f);
        
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);
        }
    }

    void HealEffect()
    {
        // Restore 4 HP
        // You'll need to add a Heal method to PlayerController
        Debug.Log("Healed 4 HP!");
        
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayHeal();
        }
    }

    /// <summary>
    /// Check if a skill is unlocked
    /// </summary>
    public bool HasSkill(string skillName)
    {
        return unlockedSkills.Exists(s => s.skillName == skillName);
    }

    /// <summary>
    /// Get cooldown remaining for skill slot
    /// </summary>
    public float GetCooldown(int slotNumber)
    {
        switch (slotNumber)
        {
            case 1: return skill1Cooldown;
            case 2: return skill2Cooldown;
            case 3: return skill3Cooldown;
            default: return 0f;
        }
    }
}

/// <summary>
/// Skill data structure
/// </summary>
[System.Serializable]
public class Skill
{
    public string skillName;
    public SkillType skillType;
    public string description;
    public int requiredLevel;
    public int skillPointCost;
    public float cooldown;
    public Sprite icon;
}

public enum SkillType
{
    Active,
    Passive
}
