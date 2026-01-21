using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public InventorySlot[] itemSlots;
    public UseItem useItem;
    public GameObject lootPrefab;
    
    public int gold;
    public TMP_Text goldText;

    [System.Serializable]
    public struct SlotSyncPair
    {
        public InventorySlot slotA;
        public InventorySlot slotB;
    }

    [SerializeField] private SlotSyncPair[] syncPairs;

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
    }

    private void Start()
    {
        foreach (var slot in itemSlots)
        {
            if (slot != null)
                slot.UpdateUI();
        }
    }

    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    public void ResetInventory()
    {
        gold = 0;
        if (goldText != null) 
            goldText.text = "0";

        foreach (var slot in itemSlots)
        {
            if (slot != null)
            {
                slot.itemSO = null;
                slot.quantity = 0;
                slot.UpdateUI();
                SyncSlots(slot);
            }
        }
    }

    public void SyncSlotsPublic(InventorySlot changedSlot)
    {
        SyncSlots(changedSlot);
    }

    private void SyncSlots(InventorySlot changedSlot)
    {
        if (syncPairs == null) return;

        foreach (var pair in syncPairs)
        {
            if (changedSlot == pair.slotA)
            {
                CopySlot(pair.slotA, pair.slotB);
                return;
            }
            else if (changedSlot == pair.slotB)
            {
                CopySlot(pair.slotB, pair.slotA);
                return;
            }
        }
    }

    private void CopySlot(InventorySlot from, InventorySlot to)
    {
        if (to == null || from == null) return;

        to.itemSO = from.itemSO;
        to.quantity = from.quantity;
        to.UpdateUI();
    }

    public void AddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold)
        {
            gold += quantity;
            if (goldText != null)
                goldText.text = gold.ToString();
            return;
        }

        if (itemSO.itemType == ItemType.Collectible)
        {
            for (int i = 0; i < quantity; i++)
            {
                bool added = false;
                foreach (var slot in itemSlots)
                {
                    if (slot.itemSO == null)
                    {
                        slot.itemSO = itemSO;
                        slot.quantity = 1;
                        SyncSlots(slot);
                        slot.UpdateUI();
                        added = true;
                        break;
                    }
                }
                if (!added) DropLoot(itemSO, 1);
            }
            return;
        }

        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);
                slot.quantity += amountToAdd;
                quantity -= amountToAdd;
                SyncSlots(slot);
                slot.UpdateUI();
                if (quantity <= 0) return;
            }
        }

        foreach (var slot in itemSlots)
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);
                slot.itemSO = itemSO;
                slot.quantity = quantity;
                SyncSlots(slot);
                slot.UpdateUI();
                return;
            }
        }

        if (quantity > 0)
        {
            DropLoot(itemSO, quantity);
        }
    }

    public void DropItem(InventorySlot slot)
    {
        if (slot.itemSO == null) return;

        DropLoot(slot.itemSO, 1);
        slot.quantity--;
        if(slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        
        slot.UpdateUI();
        SyncSlots(slot);
    }

    private void DropLoot(ItemSO itemSO, int quantity)
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Vector3 dropPosition = Vector3.zero;

        if (playerObj != null)
        {
            dropPosition = playerObj.transform.position;
        }
        else
        {
            if (Camera.main != null) 
                dropPosition = Camera.main.transform.position;
        }

        if (lootPrefab != null)
        {
            Vector3 offset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0);
            GameObject lootObj = Instantiate(lootPrefab, dropPosition + offset, Quaternion.identity);
            Loot loot = lootObj.GetComponent<Loot>();
            if (loot != null)
            {
                loot.Initialize(itemSO, quantity);
            }
        }
    }

    public void UseItem(InventorySlot slot)
    {
        if(slot.itemSO != null && slot.quantity > 0)
        {
            if (useItem != null)
            {
                useItem.ApplyItemEffects(slot.itemSO);
            }

            slot.quantity--;
            if(slot.quantity <= 0)
            {
                slot.itemSO = null;
            }
            
            slot.UpdateUI();
            SyncSlots(slot);
        }
    }
}