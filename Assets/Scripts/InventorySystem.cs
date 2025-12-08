using UnityEngine;
using System.Collections.Generic;

// InventorySystem.cs - Attach to player GameObject
// Manages equipment, consumables, and inventory
public class InventorySystem : MonoBehaviour
{
    [Header("Equipment Slots")]
    public EquipmentItem equippedWeapon;
    public EquipmentItem equippedArmor;
    public EquipmentItem equippedAccessory;

    [Header("Inventory")]
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public int maxInventorySize = 20;

    [Header("Consumables")]
    public List<ConsumableItem> consumables = new List<ConsumableItem>();

    private RPGStats rpgStats;
    private PlayerController playerController;
    private int playerNumber;

    void Start()
    {
        rpgStats = GetComponent<RPGStats>();
        playerController = GetComponent<PlayerController>();
        playerNumber = playerController.playerNumber;

        InitializeDefaultItems();
    }

    void Update()
    {
        // Quick use consumable slots (keyboard only for now)
        if (playerNumber == 1)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                UseConsumable(0);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                UseConsumable(1);
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                UseConsumable(2);
            }
        }
    }

    void InitializeDefaultItems()
    {
        // This would typically load from a database or ScriptableObjects
        // For now, we'll create some example items
    }

    /// <summary>
    /// Add item to inventory
    /// </summary>
    public bool AddItem(InventoryItem item)
    {
        // Check if inventory is full
        if (inventory.Count >= maxInventorySize)
        {
            Debug.Log("Inventory is full!");
            return false;
        }

        // Check if item is stackable
        if (item.isStackable)
        {
            // Find existing stack
            InventoryItem existingItem = inventory.Find(i => i.itemName == item.itemName);
            if (existingItem != null)
            {
                existingItem.quantity += item.quantity;
                Debug.Log($"Added {item.quantity}x {item.itemName} to existing stack");
                return true;
            }
        }

        // Add new item
        inventory.Add(item);
        Debug.Log($"Added {item.itemName} to inventory");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayCoin();
        }

        return true;
    }

    /// <summary>
    /// Remove item from inventory
    /// </summary>
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        InventoryItem item = inventory.Find(i => i.itemName == itemName);
        
        if (item == null)
        {
            Debug.Log($"{itemName} not found in inventory");
            return false;
        }

        if (item.quantity < quantity)
        {
            Debug.Log($"Not enough {itemName} in inventory");
            return false;
        }

        item.quantity -= quantity;
        
        if (item.quantity <= 0)
        {
            inventory.Remove(item);
        }

        return true;
    }

    /// <summary>
    /// Equip an equipment item
    /// </summary>
    public bool EquipItem(EquipmentItem equipment)
    {
        // Check level requirement
        if (rpgStats.currentLevel < equipment.requiredLevel)
        {
            Debug.Log($"Level {equipment.requiredLevel} required to equip {equipment.itemName}");
            return false;
        }

        EquipmentItem previousEquipment = null;

        // Equip based on type
        switch (equipment.equipmentType)
        {
            case EquipmentType.Weapon:
                previousEquipment = equippedWeapon;
                equippedWeapon = equipment;
                break;

            case EquipmentType.Armor:
                previousEquipment = equippedArmor;
                equippedArmor = equipment;
                break;

            case EquipmentType.Accessory:
                previousEquipment = equippedAccessory;
                equippedAccessory = equipment;
                break;
        }

        // Remove previous equipment bonuses
        if (previousEquipment != null)
        {
            rpgStats.RemoveEquipmentBonus(previousEquipment.stats);
        }

        // Apply new equipment bonuses
        rpgStats.AddEquipmentBonus(equipment.stats);

        Debug.Log($"Equipped {equipment.itemName}");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        return true;
    }

    /// <summary>
    /// Unequip an equipment slot
    /// </summary>
    public void UnequipItem(EquipmentType type)
    {
        EquipmentItem equipment = null;

        switch (type)
        {
            case EquipmentType.Weapon:
                equipment = equippedWeapon;
                equippedWeapon = null;
                break;

            case EquipmentType.Armor:
                equipment = equippedArmor;
                equippedArmor = null;
                break;

            case EquipmentType.Accessory:
                equipment = equippedAccessory;
                equippedAccessory = null;
                break;
        }

        if (equipment != null)
        {
            rpgStats.RemoveEquipmentBonus(equipment.stats);
            Debug.Log($"Unequipped {equipment.itemName}");
        }
    }

    /// <summary>
    /// Use a consumable item
    /// </summary>
    public bool UseConsumable(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= consumables.Count)
        {
            return false;
        }

        ConsumableItem consumable = consumables[slotIndex];
        
        if (consumable == null || consumable.quantity <= 0)
        {
            return false;
        }

        // Apply consumable effects
        ApplyConsumableEffect(consumable);

        // Decrease quantity
        consumable.quantity--;
        
        if (consumable.quantity <= 0)
        {
            consumables.RemoveAt(slotIndex);
        }

        Debug.Log($"Used {consumable.itemName}");

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayPowerUp();
        }

        return true;
    }

    /// <summary>
    /// Apply consumable item effects
    /// </summary>
    void ApplyConsumableEffect(ConsumableItem consumable)
    {
        switch (consumable.consumableType)
        {
            case ConsumableType.HealthPotion:
                // Heal player
                // You'll need to add a Heal method to PlayerController
                Debug.Log($"Healed {consumable.effectValue} HP");
                break;

            case ConsumableType.StrengthPotion:
                // Temporary strength boost
                StartCoroutine(TemporaryStatBoost("Strength", consumable.effectValue, consumable.duration));
                break;

            case ConsumableType.DefensePotion:
                // Temporary defense boost
                StartCoroutine(TemporaryStatBoost("Defense", consumable.effectValue, consumable.duration));
                break;

            case ConsumableType.SpeedPotion:
                // Temporary speed boost
                StartCoroutine(TemporaryStatBoost("Speed", consumable.effectValue, consumable.duration));
                break;

            case ConsumableType.XPBoost:
                // Temporary XP multiplier
                Debug.Log($"XP gain increased by {consumable.effectValue}% for {consumable.duration}s");
                break;
        }
    }

    /// <summary>
    /// Temporary stat boost coroutine
    /// </summary>
    System.Collections.IEnumerator TemporaryStatBoost(string statType, int boostAmount, float duration)
    {
        // Apply boost
        switch (statType)
        {
            case "Strength":
                rpgStats.equipmentStrengthBonus += boostAmount;
                break;
            case "Defense":
                rpgStats.equipmentDefenseBonus += boostAmount;
                break;
            case "Speed":
                rpgStats.equipmentAgilityBonus += boostAmount;
                break;
        }

        Debug.Log($"{statType} increased by {boostAmount} for {duration} seconds");

        yield return new WaitForSeconds(duration);

        // Remove boost
        switch (statType)
        {
            case "Strength":
                rpgStats.equipmentStrengthBonus -= boostAmount;
                break;
            case "Defense":
                rpgStats.equipmentDefenseBonus -= boostAmount;
                break;
            case "Speed":
                rpgStats.equipmentAgilityBonus -= boostAmount;
                break;
        }

        Debug.Log($"{statType} boost expired");
    }

    /// <summary>
    /// Get equipped weapon damage bonus
    /// </summary>
    public int GetWeaponDamage()
    {
        if (equippedWeapon != null)
        {
            return equippedWeapon.weaponDamage;
        }
        return 0;
    }

    /// <summary>
    /// Get total equipment defense
    /// </summary>
    public int GetTotalEquipmentDefense()
    {
        int totalDefense = 0;
        
        if (equippedWeapon != null)
            totalDefense += equippedWeapon.stats.defenseBonus;
        
        if (equippedArmor != null)
            totalDefense += equippedArmor.stats.defenseBonus;
        
        if (equippedAccessory != null)
            totalDefense += equippedAccessory.stats.defenseBonus;
        
        return totalDefense;
    }
}

/// <summary>
/// Base inventory item class
/// </summary>
[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemRarity rarity;
    public int quantity;
    public bool isStackable;
    public int sellValue;
}

/// <summary>
/// Equipment item class
/// </summary>
[System.Serializable]
public class EquipmentItem : InventoryItem
{
    public EquipmentType equipmentType;
    public int requiredLevel;
    public EquipmentStats stats;
    public int weaponDamage; // For weapons only
}

/// <summary>
/// Consumable item class
/// </summary>
[System.Serializable]
public class ConsumableItem : InventoryItem
{
    public ConsumableType consumableType;
    public int effectValue;
    public float duration; // For temporary effects
}

public enum EquipmentType
{
    Weapon,
    Armor,
    Accessory
}

public enum ConsumableType
{
    HealthPotion,
    StrengthPotion,
    DefensePotion,
    SpeedPotion,
    XPBoost
}

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Legacy,
    Basic
}
