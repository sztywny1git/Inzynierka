using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemSO itemSO;
    public int quantity;
    public Image itemImage;
    public TMP_Text quantityText;

    //public Image WholeSlot;


    private InventoryManager inventoryManager;
    private InventoryInfo inventoryInfo;
    private static ShopManager activeShop;

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        inventoryInfo = FindObjectOfType<InventoryInfo>();
    }

    private void Update()
    {
        // ŒledŸ mysz podczas hover
        if (itemSO != null && inventoryInfo != null && inventoryInfo.infoPanel.alpha > 0)
        {
            inventoryInfo.FollowMouse();
        }
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (itemSO != null && inventoryInfo != null)
        {
            inventoryInfo.ShowItemInfo(itemSO);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryInfo != null)
        {
            inventoryInfo.HideItemInfo();
        }
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
                    inventoryManager.SyncSlotsPublic(this);
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
                                inventoryManager.SyncSlotsPublic(this);
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

    /*private Color ColorFromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        else
            return Color.white; // fallback jeœli hex niepoprawny
    }

    private void ApplyRarityColor(ItemSO item)
    {
        if (itemImage == null)
            return;

        switch(item.rarity)
        {
            case Rarity.Common:
                WholeSlot.color = Color.gray;
                break;
            case Rarity.Rare:
                WholeSlot.color = ColorFromHex("#87CEFA");
                break;
            case Rarity.Epic:
                WholeSlot.color = ColorFromHex("#B026FF");
                break;
            case Rarity.Legendary:
                WholeSlot.color = new Color(1f, 0.6f, 0f); // z³oty
                break;
        }
    }*/


    public void UpdateUI()
    {
        if (quantity <= 0)
            itemSO = null;

        if (itemSO != null)
        {
            itemImage.sprite = itemSO.icon;
            itemImage.gameObject.SetActive(true);


            //ApplyRarityColor(itemSO);



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