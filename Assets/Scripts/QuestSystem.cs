using UnityEngine;
using System.Collections.Generic;
using System;

// QuestSystem.cs - Attach to player GameObject or GameManager
// Manages quests, objectives, and rewards
public class QuestSystem : MonoBehaviour
{
    [Header("Quest System")]
    public List<Quest> activeQuests = new List<Quest>();
    public List<Quest> completedQuests = new List<Quest>();
    public List<Quest> availableQuests = new List<Quest>();

    [Header("Quest Tracking")]
    public int enemiesKilled = 0;
    public int coinsCollected = 0;
    public int candiesCollected = 0;
    public int healthPickupsCollected = 0;

    private RPGStats rpgStats;
    private int playerNumber;

    // Events
    public event Action<Quest> OnQuestStarted;
    public event Action<Quest> OnQuestCompleted;
    public event Action<Quest, QuestObjective> OnObjectiveCompleted;

    void Start()
    {
        rpgStats = GetComponent<RPGStats>();
        if (rpgStats != null)
        {
            playerNumber = GetComponent<PlayerController>().playerNumber;
        }

        InitializeQuests();
    }

    void InitializeQuests()
    {
        // Create example quests
        availableQuests = new List<Quest>
        {
            // Tutorial Quests
            new Quest
            {
                questId = "tutorial_001",
                questName = "First Steps",
                description = "Learn the basics of combat and movement",
                questType = QuestType.Tutorial,
                requiredLevel = 1,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.KillEnemies,
                        targetCount = 3,
                        description = "Defeat 3 enemies"
                    },
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.CollectCoins,
                        targetCount = 10,
                        description = "Collect 10 coins"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 50,
                    coinReward = 100,
                    itemReward = null
                }
            },

            // Main Quests
            new Quest
            {
                questId = "main_001",
                questName = "The Candy Thief",
                description = "A mysterious enemy is stealing candy. Defeat candy enemies to investigate.",
                questType = QuestType.Main,
                requiredLevel = 3,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.KillEnemies,
                        targetCount = 10,
                        description = "Defeat 10 candy enemies"
                    },
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.CollectCandies,
                        targetCount = 20,
                        description = "Collect 20 candies"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 200,
                    coinReward = 500,
                    itemReward = null
                }
            },

            new Quest
            {
                questId = "main_002",
                questName = "Skeleton King's Reign",
                description = "The Skeleton King threatens the land. Prepare yourself and face him in battle.",
                questType = QuestType.Main,
                requiredLevel = 8,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.DefeatBoss,
                        targetCount = 1,
                        description = "Defeat the Skeleton Boss"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 1000,
                    coinReward = 2000,
                    itemReward = null
                }
            },

            // Side Quests
            new Quest
            {
                questId = "side_001",
                questName = "Treasure Hunter",
                description = "Collect coins scattered throughout the land",
                questType = QuestType.Side,
                requiredLevel = 2,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.CollectCoins,
                        targetCount = 50,
                        description = "Collect 50 coins"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 100,
                    coinReward = 300,
                    itemReward = null
                }
            },

            new Quest
            {
                questId = "side_002",
                questName = "Health Seeker",
                description = "Find and collect health pickups",
                questType = QuestType.Side,
                requiredLevel = 1,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.CollectHealthPickups,
                        targetCount = 5,
                        description = "Collect 5 health pickups"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 75,
                    coinReward = 200,
                    itemReward = null
                }
            },

            // Challenge Quests
            new Quest
            {
                questId = "challenge_001",
                questName = "Unstoppable",
                description = "Complete a difficult combat challenge",
                questType = QuestType.Challenge,
                requiredLevel = 5,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.KillEnemies,
                        targetCount = 25,
                        description = "Defeat 25 enemies without dying"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 500,
                    coinReward = 1000,
                    itemReward = null
                }
            },

            // Daily Quests
            new Quest
            {
                questId = "daily_001",
                questName = "Daily Grind",
                description = "Complete daily objectives for bonus rewards",
                questType = QuestType.Daily,
                requiredLevel = 1,
                objectives = new List<QuestObjective>
                {
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.KillEnemies,
                        targetCount = 15,
                        description = "Defeat 15 enemies"
                    },
                    new QuestObjective
                    {
                        objectiveType = ObjectiveType.CollectCoins,
                        targetCount = 30,
                        description = "Collect 30 coins"
                    }
                },
                rewards = new QuestReward
                {
                    xpReward = 300,
                    coinReward = 500,
                    itemReward = null
                }
            }
        };
    }

    /// <summary>
    /// Start a quest
    /// </summary>
    public bool StartQuest(Quest quest)
    {
        // Check if player meets level requirement
        if (rpgStats.currentLevel < quest.requiredLevel)
        {
            Debug.Log($"Level {quest.requiredLevel} required for quest: {quest.questName}");
            return false;
        }

        // Check if quest is already active
        if (activeQuests.Contains(quest))
        {
            Debug.Log($"Quest already active: {quest.questName}");
            return false;
        }

        // Check if quest is already completed
        if (completedQuests.Contains(quest))
        {
            Debug.Log($"Quest already completed: {quest.questName}");
            return false;
        }

        // Start the quest
        quest.isActive = true;
        quest.startTime = Time.time;
        activeQuests.Add(quest);

        OnQuestStarted?.Invoke(quest);

        Debug.Log($"Started quest: {quest.questName}");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        return true;
    }

    /// <summary>
    /// Update quest progress for enemy kills
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        UpdateQuestProgress(ObjectiveType.KillEnemies, 1);
    }

    /// <summary>
    /// Update quest progress for coin collection
    /// </summary>
    public void OnCoinCollected()
    {
        coinsCollected++;
        UpdateQuestProgress(ObjectiveType.CollectCoins, 1);
    }

    /// <summary>
    /// Update quest progress for candy collection
    /// </summary>
    public void OnCandyCollected()
    {
        candiesCollected++;
        UpdateQuestProgress(ObjectiveType.CollectCandies, 1);
    }

    /// <summary>
    /// Update quest progress for health pickup collection
    /// </summary>
    public void OnHealthPickupCollected()
    {
        healthPickupsCollected++;
        UpdateQuestProgress(ObjectiveType.CollectHealthPickups, 1);
    }

    /// <summary>
    /// Update quest progress for boss defeat
    /// </summary>
    public void OnBossDefeated()
    {
        UpdateQuestProgress(ObjectiveType.DefeatBoss, 1);
    }

    /// <summary>
    /// Update quest progress based on objective type
    /// </summary>
    void UpdateQuestProgress(ObjectiveType objectiveType, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == objectiveType && !objective.isCompleted)
                {
                    objective.currentCount += amount;

                    // Check if objective is completed
                    if (objective.currentCount >= objective.targetCount)
                    {
                        objective.isCompleted = true;
                        OnObjectiveCompleted?.Invoke(quest, objective);
                        Debug.Log($"Objective completed: {objective.description}");

                        if (AudioManager.instance != null)
                        {
                            AudioManager.instance.PlayCoin();
                        }
                    }

                    // Check if entire quest is completed
                    if (IsQuestCompleted(quest))
                    {
                        CompleteQuest(quest);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if all objectives are completed
    /// </summary>
    bool IsQuestCompleted(Quest quest)
    {
        foreach (var objective in quest.objectives)
        {
            if (!objective.isCompleted)
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Complete a quest and give rewards
    /// </summary>
    void CompleteQuest(Quest quest)
    {
        quest.isActive = false;
        quest.isCompleted = true;
        quest.completionTime = Time.time;

        // Move from active to completed
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        // Give rewards
        GiveQuestRewards(quest);

        OnQuestCompleted?.Invoke(quest);

        Debug.Log($"Quest completed: {quest.questName}");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        // Show completion notification
        if (RPGUIManager.instance != null)
        {
            RPGUIManager.instance.ShowQuestCompleteNotification(quest);
        }
    }

    /// <summary>
    /// Give quest rewards to player
    /// </summary>
    void GiveQuestRewards(Quest quest)
    {
        QuestReward reward = quest.rewards;

        // XP reward
        if (reward.xpReward > 0 && rpgStats != null)
        {
            rpgStats.AddXP(reward.xpReward);
            Debug.Log($"Gained {reward.xpReward} XP");
        }

        // Coin reward
        if (reward.coinReward > 0 && GameManager.instance != null)
        {
            GameManager.instance.AddScore(reward.coinReward, playerNumber);
            Debug.Log($"Gained {reward.coinReward} coins");
        }

        // Item reward
        if (reward.itemReward != null)
        {
            InventorySystem inventory = GetComponent<InventorySystem>();
            if (inventory != null)
            {
                inventory.AddItem(reward.itemReward);
                Debug.Log($"Received item: {reward.itemReward.itemName}");
            }
        }
    }

    /// <summary>
    /// Get quest by ID
    /// </summary>
    public Quest GetQuestById(string questId)
    {
        Quest quest = availableQuests.Find(q => q.questId == questId);
        if (quest == null)
        {
            quest = activeQuests.Find(q => q.questId == questId);
        }
        if (quest == null)
        {
            quest = completedQuests.Find(q => q.questId == questId);
        }
        return quest;
    }

    /// <summary>
    /// Get quest progress as percentage
    /// </summary>
    public float GetQuestProgress(Quest quest)
    {
        int totalObjectives = quest.objectives.Count;
        int completedObjectives = 0;

        foreach (var objective in quest.objectives)
        {
            if (objective.isCompleted)
            {
                completedObjectives++;
            }
        }

        return (float)completedObjectives / totalObjectives;
    }
}

/// <summary>
/// Quest data structure
/// </summary>
[System.Serializable]
public class Quest
{
    public string questId;
    public string questName;
    public string description;
    public QuestType questType;
    public int requiredLevel;
    public List<QuestObjective> objectives;
    public QuestReward rewards;
    
    public bool isActive;
    public bool isCompleted;
    public float startTime;
    public float completionTime;
}

/// <summary>
/// Quest objective structure
/// </summary>
[System.Serializable]
public class QuestObjective
{
    public ObjectiveType objectiveType;
    public string description;
    public int targetCount;
    public int currentCount;
    public bool isCompleted;
}

/// <summary>
/// Quest reward structure
/// </summary>
[System.Serializable]
public class QuestReward
{
    public int xpReward;
    public int coinReward;
    public InventoryItem itemReward;
}

public enum QuestType
{
    Main,
    Side,
    Tutorial,
    Daily,
    Challenge
}

public enum ObjectiveType
{
    KillEnemies,
    DefeatBoss,
    CollectCoins,
    CollectCandies,
    CollectHealthPickups,
    ReachLocation,
    TalkToNPC
}
