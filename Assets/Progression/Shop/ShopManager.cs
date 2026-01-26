using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShopSlot[] shopSlots;
    [SerializeField] private InventoryManager inventoryManager;
    
    [Header("Shop Rules")]
    [SerializeField] private float priceMultiplier = 1.2f; 

    [Header("Restock Settings")]
    [SerializeField] private LootTableSO restockTable;

    private List<ShopItems> currentShopSource;

    public void PopulateShopItems(List<ShopItems> shopItems)
    {
        currentShopSource = shopItems;

        for (int i = 0; i < shopItems.Count && i < shopSlots.Length; i++)
        {
            ShopItems shopItem = shopItems[i];
            
            int finalPrice = Mathf.CeilToInt(shopItem.itemSO.value * priceMultiplier);
            
            shopItem.price = finalPrice; 
            
            shopSlots[i].Initialize(shopItem.itemSO, finalPrice);
            shopSlots[i].gameObject.SetActive(true);
        }

        for (int i = shopItems.Count; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void TryBuyItem(ItemSO itemSO, int price)
    {
        if (itemSO != null && inventoryManager.gold >= price)
        {
            if (HasSpaceForItem(itemSO))
            {
                inventoryManager.gold -= price;
                inventoryManager.goldText.text = inventoryManager.gold.ToString();
                inventoryManager.AddItem(itemSO, 1);

                RestockSlot(itemSO);
            }
        }
    }

    private void RestockSlot(ItemSO purchasedItem)
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (shopSlots[i].gameObject.activeSelf && shopSlots[i].itemSO == purchasedItem)
            {
                if (restockTable != null)
                {
                    ItemSO newItem = restockTable.GetRandomItem();
                    
                    if (newItem != null)
                    {
                        int restockPrice = Mathf.CeilToInt(newItem.value * priceMultiplier);
                        
                        shopSlots[i].Initialize(newItem, restockPrice);

                        if (currentShopSource != null && i < currentShopSource.Count)
                        {
                            currentShopSource[i].itemSO = newItem;
                            currentShopSource[i].price = restockPrice;
                        }
                    }
                    else
                    {
                        shopSlots[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    shopSlots[i].gameObject.SetActive(false);
                }
                
                return;
            }
        }
    }

    private bool HasSpaceForItem(ItemSO itemSO)
    {
        foreach (var slot in inventoryManager.itemSlots)
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
                return true;
            else if (slot.itemSO == null)
                return true;
        }
        return false;
    }

    public void SellItem(ItemSO itemSO)
    {
        if (itemSO == null)
            return;

        inventoryManager.gold += itemSO.value;
        inventoryManager.goldText.text = inventoryManager.gold.ToString();
    }
}


[System.Serializable]
public class ShopItems
{
    public ItemSO itemSO;
    public int price;
}