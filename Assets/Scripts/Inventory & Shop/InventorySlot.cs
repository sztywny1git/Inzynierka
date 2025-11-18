using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public int quantity;
    public Image itemImage;
    public TMP_Text quantityText;

    private InventoryManager inventoryManager;
    private static ShopManager activeShop;

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
    }

    private void OnEnable()
    {
        ShopKeeper.OnShopStateChanged += HandleShopStateChanged;
    }

    private void OnDisable()
    {
        ShopKeeper.OnShopStateChanged -= HandleShopStateChanged;
    }

    private void HandleShopStateChanged(ShopManager shopManager, bool isOpen)
    {
        activeShop = isOpen ? shopManager : null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (quantity > 0)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (activeShop != null)
                {
                    activeShop.SellItem(itemSO);
                    quantity--;
                    UpdateUI();
                }
                else
                {
                    // SprawdŸ czy to wyposa¿enie
                    if (IsEquipment(itemSO.itemType))
                    {
                        // Próbuj za³o¿yæ wyposa¿enie
                        if (EquipmentManager.Instance != null)
                        {
                            if (EquipmentManager.Instance.EquipItem(itemSO))
                            {
                                // Usuñ z ekwipunku po za³o¿eniu
                                quantity--;
                                if (quantity <= 0)
                                {
                                    itemSO = null;
                                }
                                UpdateUI();
                            }
                        }
                    }
                    else
                    {
                        // U¿yj jako consumable
                        inventoryManager.UseItem(this);
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventoryManager.DropItem(this);
            }
        }
    }

    private bool IsEquipment(ItemType type)
    {
        return type == ItemType.Ring ||
               type == ItemType.Weapon ||
               type == ItemType.Helmet ||
               type == ItemType.Chestplate ||
               type == ItemType.Boots;
    }

    public void UpdateUI()
    {
        if (quantity <= 0)
            itemSO = null;

        if (itemSO != null)
        {
            itemImage.sprite = itemSO.icon;
            itemImage.gameObject.SetActive(true);

            // Nie pokazuj liczby dla equipment
            if (IsEquipment(itemSO.itemType))
            {
                quantityText.text = "";
            }
            else
            {
                quantityText.text = quantity.ToString();
            }
        }
        else
        {
            itemImage.gameObject.SetActive(false);
            quantityText.text = "";
        }
    }
}