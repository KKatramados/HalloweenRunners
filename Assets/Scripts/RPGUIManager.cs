using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// RPGUIManager.cs - Attach to Canvas GameObject
// Manages all RPG-related UI elements
public class RPGUIManager : MonoBehaviour
{
    public static RPGUIManager instance;

    [Header("Player 1 RPG UI")]
    public TextMeshProUGUI p1LevelText;
    public Image p1XPBar;
    public TextMeshProUGUI p1XPText;
    public GameObject p1StatsPanel;
    public TextMeshProUGUI p1StrengthText;
    public TextMeshProUGUI p1DefenseText;
    public TextMeshProUGUI p1VitalityText;
    public TextMeshProUGUI p1AgilityText;
    public TextMeshProUGUI p1StatPointsText;

    [Header("Player 2 RPG UI")]
    public TextMeshProUGUI p2LevelText;
    public Image p2XPBar;
    public TextMeshProUGUI p2XPText;
    public GameObject p2StatsPanel;
    public TextMeshProUGUI p2StrengthText;
    public TextMeshProUGUI p2DefenseText;
    public TextMeshProUGUI p2VitalityText;
    public TextMeshProUGUI p2AgilityText;
    public TextMeshProUGUI p2StatPointsText;

    [Header("Skill UI")]
    public GameObject skillTreePanel;
    public Transform skillTreeContent;
    public GameObject skillButtonPrefab;

    [Header("Inventory UI")]
    public GameObject inventoryPanel;
    public Transform inventoryGrid;
    public GameObject inventorySlotPrefab;
    public TextMeshProUGUI inventoryInfoText;

    [Header("Quest UI")]
    public GameObject questPanel;
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public TextMeshProUGUI activeQuestText;

    [Header("Notification UI")]
    public GameObject notificationPanel;
    public TextMeshProUGUI notificationText;
    public float notificationDuration = 3f;

    [Header("Skill Cooldown UI")]
    public Image skill1CooldownOverlay;
    public Image skill2CooldownOverlay;
    public Image skill3CooldownOverlay;
    public TextMeshProUGUI skill1CooldownText;
    public TextMeshProUGUI skill2CooldownText;
    public TextMeshProUGUI skill3CooldownText;

    private bool isStatsPanel1Open = false;
    private bool isStatsPanel2Open = false;
    private bool isSkillTreeOpen = false;
    private bool isInventoryOpen = false;
    private bool isQuestPanelOpen = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide all panels initially
        if (p1StatsPanel != null) p1StatsPanel.SetActive(false);
        if (p2StatsPanel != null) p2StatsPanel.SetActive(false);
        if (skillTreePanel != null) skillTreePanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        if (questPanel != null) questPanel.SetActive(false);
        if (notificationPanel != null) notificationPanel.SetActive(false);

        // Hide Player 2 UI on mobile
        if (Application.isMobilePlatform)
        {
            SetPlayer2UIVisibility(false);
        }
    }

    void Update()
    {
        // Toggle UI panels with keyboard
        if (Input.GetKeyDown(KeyCode.C))
        {
            ToggleStatsPanel(1);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            ToggleSkillTree();
        }
        else if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleQuestPanel();
        }

        // Update skill cooldowns
        UpdateSkillCooldowns();
    }

    void SetPlayer2UIVisibility(bool visible)
    {
        if (p2LevelText != null) p2LevelText.gameObject.SetActive(visible);
        if (p2XPBar != null) p2XPBar.gameObject.SetActive(visible);
        if (p2XPText != null) p2XPText.gameObject.SetActive(visible);
    }

    #region XP and Level UI
    /// <summary>
    /// Update XP bar for a player
    /// </summary>
    public void UpdateXPBar(int playerNumber, int currentXP, int xpToNext)
    {
        Image xpBar = playerNumber == 1 ? p1XPBar : p2XPBar;
        TextMeshProUGUI xpText = playerNumber == 1 ? p1XPText : p2XPText;

        if (xpBar != null)
        {
            float fillAmount = (float)currentXP / xpToNext;
            xpBar.fillAmount = fillAmount;
        }

        if (xpText != null)
        {
            xpText.text = $"{currentXP}/{xpToNext} XP";
        }
    }

    /// <summary>
    /// Update level display
    /// </summary>
    public void UpdateLevelDisplay(int playerNumber, int level)
    {
        TextMeshProUGUI levelText = playerNumber == 1 ? p1LevelText : p2LevelText;
        
        if (levelText != null)
        {
            levelText.text = $"LVL {level}";
        }
    }

    /// <summary>
    /// Show level up notification
    /// </summary>
    public void ShowLevelUpNotification(int playerNumber, int newLevel)
    {
        string message = $"Player {playerNumber} reached Level {newLevel}!";
        ShowNotification(message, Color.yellow);
    }
    #endregion

    #region Stats UI
    /// <summary>
    /// Toggle stats panel
    /// </summary>
    public void ToggleStatsPanel(int playerNumber)
    {
        if (playerNumber == 1)
        {
            isStatsPanel1Open = !isStatsPanel1Open;
            if (p1StatsPanel != null)
            {
                p1StatsPanel.SetActive(isStatsPanel1Open);
            }
        }
        else
        {
            isStatsPanel2Open = !isStatsPanel2Open;
            if (p2StatsPanel != null)
            {
                p2StatsPanel.SetActive(isStatsPanel2Open);
            }
        }
    }

    /// <summary>
    /// Update stats display
    /// </summary>
    public void UpdateStatsDisplay(int playerNumber, RPGStats stats)
    {
        if (playerNumber == 1)
        {
            if (p1LevelText != null) p1LevelText.text = $"LVL {stats.currentLevel}";
            if (p1StrengthText != null) p1StrengthText.text = $"STR: {stats.strength}";
            if (p1DefenseText != null) p1DefenseText.text = $"DEF: {stats.defense}";
            if (p1VitalityText != null) p1VitalityText.text = $"VIT: {stats.vitality}";
            if (p1AgilityText != null) p1AgilityText.text = $"AGI: {stats.agility}";
            if (p1StatPointsText != null) p1StatPointsText.text = $"Points: {stats.availableStatPoints}";
        }
        else
        {
            if (p2LevelText != null) p2LevelText.text = $"LVL {stats.currentLevel}";
            if (p2StrengthText != null) p2StrengthText.text = $"STR: {stats.strength}";
            if (p2DefenseText != null) p2DefenseText.text = $"DEF: {stats.defense}";
            if (p2VitalityText != null) p2VitalityText.text = $"VIT: {stats.vitality}";
            if (p2AgilityText != null) p2AgilityText.text = $"AGI: {stats.agility}";
            if (p2StatPointsText != null) p2StatPointsText.text = $"Points: {stats.availableStatPoints}";
        }

        UpdateXPBar(playerNumber, stats.currentXP, stats.xpToNextLevel);
    }
    #endregion

    #region Skill Tree UI
    /// <summary>
    /// Toggle skill tree panel
    /// </summary>
    public void ToggleSkillTree()
    {
        isSkillTreeOpen = !isSkillTreeOpen;
        
        if (skillTreePanel != null)
        {
            skillTreePanel.SetActive(isSkillTreeOpen);
            
            if (isSkillTreeOpen)
            {
                PopulateSkillTree();
            }
        }
    }

    /// <summary>
    /// Populate skill tree with available skills
    /// </summary>
    void PopulateSkillTree()
    {
        if (skillTreeContent == null || skillButtonPrefab == null)
            return;

        // Clear existing buttons
        foreach (Transform child in skillTreeContent)
        {
            Destroy(child.gameObject);
        }

        // Get player's skill tree
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null)
        {
            SkillTree skillTree = player1.GetComponent<SkillTree>();
            if (skillTree != null)
            {
                foreach (var skill in skillTree.availableSkills)
                {
                    CreateSkillButton(skill, skillTree);
                }
            }
        }
    }

    void CreateSkillButton(Skill skill, SkillTree skillTree)
    {
        GameObject buttonObj = Instantiate(skillButtonPrefab, skillTreeContent);
        Button button = buttonObj.GetComponent<Button>();
        
        // Set button text
        TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            bool isUnlocked = skillTree.unlockedSkills.Contains(skill);
            string statusText = isUnlocked ? "[UNLOCKED]" : $"[Level {skill.requiredLevel}]";
            buttonText.text = $"{skill.skillName}\n{statusText}";
        }

        // Add click listener
        button.onClick.AddListener(() => {
            if (!skillTree.unlockedSkills.Contains(skill))
            {
                skillTree.UnlockSkill(skill);
                PopulateSkillTree(); // Refresh
            }
        });
    }

    /// <summary>
    /// Update skill cooldown displays
    /// </summary>
    void UpdateSkillCooldowns()
    {
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null)
        {
            SkillTree skillTree = player1.GetComponent<SkillTree>();
            if (skillTree != null)
            {
                UpdateSkillCooldownUI(skillTree.GetCooldown(1), skill1CooldownOverlay, skill1CooldownText);
                UpdateSkillCooldownUI(skillTree.GetCooldown(2), skill2CooldownOverlay, skill2CooldownText);
                UpdateSkillCooldownUI(skillTree.GetCooldown(3), skill3CooldownOverlay, skill3CooldownText);
            }
        }
    }

    void UpdateSkillCooldownUI(float cooldown, Image overlay, TextMeshProUGUI text)
    {
        if (overlay != null)
        {
            overlay.fillAmount = cooldown > 0 ? 1f : 0f;
        }
        
        if (text != null)
        {
            text.text = cooldown > 0 ? $"{cooldown:F1}s" : "";
        }
    }
    #endregion

    #region Inventory UI
    /// <summary>
    /// Toggle inventory panel
    /// </summary>
    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
            
            if (isInventoryOpen)
            {
                PopulateInventory();
            }
        }
    }

    /// <summary>
    /// Populate inventory grid
    /// </summary>
    void PopulateInventory()
    {
        if (inventoryGrid == null || inventorySlotPrefab == null)
            return;

        // Clear existing slots
        foreach (Transform child in inventoryGrid)
        {
            Destroy(child.gameObject);
        }

        // Get player's inventory
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null)
        {
            InventorySystem inventory = player1.GetComponent<InventorySystem>();
            if (inventory != null)
            {
                foreach (var item in inventory.inventory)
                {
                    CreateInventorySlot(item);
                }
            }
        }
    }

    void CreateInventorySlot(InventoryItem item)
    {
        GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryGrid);
        
        // Set item icon and quantity
        Image iconImage = slotObj.GetComponentInChildren<Image>();
        if (iconImage != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
        }

        TextMeshProUGUI quantityText = slotObj.GetComponentInChildren<TextMeshProUGUI>();
        if (quantityText != null && item.isStackable)
        {
            quantityText.text = item.quantity.ToString();
        }
    }
    #endregion

    #region Quest UI
    /// <summary>
    /// Toggle quest panel
    /// </summary>
    public void ToggleQuestPanel()
    {
        isQuestPanelOpen = !isQuestPanelOpen;
        
        if (questPanel != null)
        {
            questPanel.SetActive(isQuestPanelOpen);
            
            if (isQuestPanelOpen)
            {
                PopulateQuestList();
            }
        }
    }

    /// <summary>
    /// Populate quest list
    /// </summary>
    void PopulateQuestList()
    {
        if (questListContent == null || questEntryPrefab == null)
            return;

        // Clear existing entries
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        // Get player's quests
        GameObject player1 = GameObject.FindGameObjectWithTag("Player");
        if (player1 != null)
        {
            QuestSystem questSystem = player1.GetComponent<QuestSystem>();
            if (questSystem != null)
            {
                // Show active quests
                foreach (var quest in questSystem.activeQuests)
                {
                    CreateQuestEntry(quest, questSystem);
                }

                // Show available quests
                foreach (var quest in questSystem.availableQuests)
                {
                    CreateQuestEntry(quest, questSystem);
                }
            }
        }
    }

    void CreateQuestEntry(Quest quest, QuestSystem questSystem)
    {
        GameObject entryObj = Instantiate(questEntryPrefab, questListContent);
        
        TextMeshProUGUI questText = entryObj.GetComponentInChildren<TextMeshProUGUI>();
        if (questText != null)
        {
            string status = quest.isActive ? "[ACTIVE]" : "[AVAILABLE]";
            float progress = questSystem.GetQuestProgress(quest) * 100f;
            questText.text = $"{status} {quest.questName}\n{progress:F0}% Complete";
        }

        Button button = entryObj.GetComponent<Button>();
        if (button != null && !quest.isActive)
        {
            button.onClick.AddListener(() => {
                questSystem.StartQuest(quest);
                PopulateQuestList(); // Refresh
            });
        }
    }

    /// <summary>
    /// Update active quest tracker
    /// </summary>
    public void UpdateActiveQuestDisplay(Quest quest)
    {
        if (activeQuestText != null && quest != null)
        {
            string objectivesText = "";
            foreach (var objective in quest.objectives)
            {
                string checkmark = objective.isCompleted ? "[✓]" : "[ ]";
                objectivesText += $"{checkmark} {objective.description} ({objective.currentCount}/{objective.targetCount})\n";
            }
            
            activeQuestText.text = $"{quest.questName}\n{objectivesText}";
        }
    }

    /// <summary>
    /// Show quest complete notification
    /// </summary>
    public void ShowQuestCompleteNotification(Quest quest)
    {
        string message = $"Quest Complete: {quest.questName}\n+{quest.rewards.xpReward} XP";
        ShowNotification(message, Color.green);
    }
    #endregion

    #region Notifications
    /// <summary>
    /// Show a notification message
    /// </summary>
    public void ShowNotification(string message, Color color)
    {
        if (notificationPanel == null || notificationText == null)
            return;

        notificationText.text = message;
        notificationText.color = color;
        notificationPanel.SetActive(true);

        StartCoroutine(HideNotificationAfterDelay());
    }

    IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }
    #endregion
}
