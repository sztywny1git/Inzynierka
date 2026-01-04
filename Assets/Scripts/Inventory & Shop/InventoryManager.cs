using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] itemSlots;
    public UseItem useItem;
    public int gold;
    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;

    [System.Serializable]
    public struct SlotSyncPair
    {
        public InventorySlot slotA;
        public InventorySlot slotB;
    }

    [SerializeField] private SlotSyncPair[] syncPairs;


    private void Start()
    {
        foreach (var slot in itemSlots)
        {
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

    public void SyncSlotsPublic(InventorySlot changedSlot)//zeby korzystac w innych plikach
    {
        SyncSlots(changedSlot);
    }

    private void SyncSlots(InventorySlot changedSlot)
    {
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


    /*private void CopySlot(InventorySlot from, InventorySlot to)
    {
        bool changed = to.itemSO != from.itemSO || to.quantity != from.quantity;

        to.itemSO = from.itemSO;
        to.quantity = from.quantity;

        if (changed)
            to.UpdateUI();
    }*/
    private void CopySlot(InventorySlot from, InventorySlot to)
    {
        to.itemSO = from.itemSO;
        to.quantity = from.quantity;
        to.UpdateUI();
    }




    public void AddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold)
        {
           gold += quantity;
           goldText.text = gold.ToString();
           return;
        }

        foreach (var slot in itemSlots) // it is the same item and there is space left
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(availableSpace, quantity);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                SyncSlots(slot);

                slot.UpdateUI();
                

                if (quantity <= 0)
                    return; // All items have been added
            }
        }

            foreach (var slot in itemSlots) //if items remain then we look for empty slots
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
        DropLoot(slot.itemSO, 1);//dropam tylko 1 item
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
        Loot loot = Instantiate(lootPrefab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(itemSO, quantity);
    }

    public void UseItem(InventorySlot slot)
    {
        if(slot.itemSO != null && slot.quantity > 0)
        {
            useItem.ApplyItemEffects(slot.itemSO);

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
