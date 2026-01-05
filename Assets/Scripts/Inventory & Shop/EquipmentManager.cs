using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Equipment Slots")]
    public EquipmentSlot ringSlot1;
    public EquipmentSlot ringSlot2;
    public EquipmentSlot weaponSlot;
    public EquipmentSlot helmetSlot;
    public EquipmentSlot chestplateSlot;
    public EquipmentSlot legsSlot;
    public EquipmentSlot bootsSlot;

    private Dictionary<ItemType, List<EquipmentSlot>> equipmentSlots;

    [SerializeField] private PlayerStats playerStats;


    private void Awake()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }

        if (playerStats == null)
        {
            Debug.LogError("PlayerStats nie został przypisany ani znaleziony!");
        }

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        equipmentSlots = new Dictionary<ItemType, List<EquipmentSlot>>
        {
            { ItemType.Ring, new List<EquipmentSlot> { ringSlot1, ringSlot2 } },
            { ItemType.Weapon, new List<EquipmentSlot> { weaponSlot } },
            { ItemType.Helmet, new List<EquipmentSlot> { helmetSlot } },
            { ItemType.Chestplate, new List<EquipmentSlot> { chestplateSlot } },
            { ItemType.Legs, new List<EquipmentSlot> { legsSlot } },
            { ItemType.Boots, new List<EquipmentSlot> { bootsSlot } }
        };
    }

    private void Start()
    {
        foreach (var slotList in equipmentSlots.Values)
        {
            foreach (var slot in slotList)
            {
                if (slot != null)
                {
                    slot.UpdateUI();
                }
            }
        }
    }

    public bool EquipItem(ItemSO itemSO)
    {
        if (itemSO.itemType == ItemType.Consumable)
            return false;

        if (!equipmentSlots.ContainsKey(itemSO.itemType))
            return false;

        List<EquipmentSlot> slotsForType = equipmentSlots[itemSO.itemType];

        // Sprawd� czy ju� nie mamy tego itemu za�o�onego
        foreach (var slot in slotsForType)
        {
            if (slot != null && slot.equippedItem == itemSO)
            {
                Debug.Log("Ten przedmiot jest ju� za�o�ony!");
                return false;
            }
        }

        // Znajd� wolny slot dla tego typu
        foreach (var slot in slotsForType)
        {
            if (slot != null && slot.equippedItem == null)
            {
                slot.Equip(itemSO);
                ApplyEquipmentStats(itemSO, true);
                return true;
            }
        }

        Debug.Log($"Brak wolnych slot�w na {itemSO.itemType}!");
        return false;
    }

    public void UnequipItem(EquipmentSlot slot)
    {
        if (slot.equippedItem != null)
        {
            ItemSO itemToUnequip = slot.equippedItem;

            // Dodaj z powrotem do ekwipunku
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager != null)
            {
                inventoryManager.AddItem(itemToUnequip, 1);
            }

            // Usu� statystyki
            ApplyEquipmentStats(itemToUnequip, false);

            slot.Unequip();
        }
    }

    private void ApplyEquipmentStats(ItemSO itemSO, bool apply)
    {
        /*PlayerStats playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found!");
            return;
        }*/

        string source = $"Equipment_{itemSO.name}";

        if (apply)
        {
            // Dodaj modyfikatory z ekwipunku
            if (itemSO.speed != 0)
            {
                playerStats.MoveSpeed.AddModifier(
                    new StatModifier(itemSO.speed, true, source)
                );
            }

            if (itemSO.currentHearts != 0)//zmieniamy z maxHearts bo przechodzimy po prostu na currenthearts
            {
                /*playerStats.Health.AddModifier(
                    new StatModifier(itemSO.maxHearts, true, source)
                );
                // Zaktualizuj maksymalne zdrowie
                playerStats.SetCurrentHealth(playerStats.CurrentHealth);*/
                playerStats.Health.AddModifier(
                    new StatModifier(itemSO.currentHearts, true, source)
                );
            }

            if (itemSO.damage != 0)
            {
                playerStats.Damage.AddModifier(
                    new StatModifier(itemSO.damage, true, source)
                );
            }

            if (itemSO.fireRate != 0)
            {
                playerStats.AttackSpeed.AddModifier(
                    new StatModifier(itemSO.fireRate, true, source)
                );
            }
            if (itemSO.Resource != 0)
            {
                playerStats.Resource.AddModifier(
                    new StatModifier(itemSO.Resource, true, source)
                );
            }
            if (itemSO.armor != 0)
            {
                playerStats.Armor.AddModifier(
                    new StatModifier(itemSO.armor, true, source)
                );
            }
            if (itemSO.CriticalDamage != 0)
            {
                playerStats.CriticalDamage.AddModifier(
                    new StatModifier(itemSO.CriticalDamage, true, source)
                );
            }
            if (itemSO.CriticalChance != 0)
            {
                playerStats.CriticalChance.AddModifier(
                    new StatModifier(itemSO.CriticalChance, true, source)
                );
            }



        }
        else
        {
            // Usuń modyfikatory z ekwipunku
            playerStats.MoveSpeed.RemoveModifierBySource(source);
            playerStats.Health.RemoveModifierBySource(source);
            playerStats.Damage.RemoveModifierBySource(source);
            playerStats.AttackSpeed.RemoveModifierBySource(source);

            playerStats.Armor.RemoveModifierBySource(source);
            playerStats.Resource.RemoveModifierBySource(source);
            playerStats.CriticalChance.RemoveModifierBySource(source);
            playerStats.CriticalDamage.RemoveModifierBySource(source);

            // Upewnij się, że aktualne zdrowie nie przekracza nowego maksimum
            if (playerStats.CurrentHealth > playerStats.Health.FinalValue)
            {
                playerStats.SetCurrentHealth(playerStats.Health.FinalValue);
            }
        }
    }
}