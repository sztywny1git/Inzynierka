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

    private void Awake()
    {
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

        // SprawdŸ czy ju¿ nie mamy tego itemu za³o¿onego
        foreach (var slot in slotsForType)
        {
            if (slot != null && slot.equippedItem == itemSO)
            {
                Debug.Log("Ten przedmiot jest ju¿ za³o¿ony!");
                return false;
            }
        }

        // ZnajdŸ wolny slot dla tego typu
        foreach (var slot in slotsForType)
        {
            if (slot != null && slot.equippedItem == null)
            {
                slot.Equip(itemSO);
                ApplyEquipmentStats(itemSO, true);
                return true;
            }
        }

        Debug.Log($"Brak wolnych slotów na {itemSO.itemType}!");
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

            // Usuñ statystyki
            ApplyEquipmentStats(itemToUnequip, false);

            slot.Unequip();
        }
    }

    private void ApplyEquipmentStats(ItemSO itemSO, bool apply)
    {
        float multiplier = apply ? 1f : -1f;

        if (itemSO.speed != 0)
            StatsManager.Instance.UpdateMaxSpeed(itemSO.speed * multiplier);

        if (itemSO.maxHearts != 0)
            StatsManager.Instance.UpdateMaxHealth((int)(itemSO.maxHearts * multiplier));

        if (itemSO.damage != 0)
            StatsManager.Instance.bulletDamage += (int)(itemSO.damage * multiplier);

        if (itemSO.fireRate != 0)
            StatsManager.Instance.fireRate += itemSO.fireRate * multiplier;
    }
}