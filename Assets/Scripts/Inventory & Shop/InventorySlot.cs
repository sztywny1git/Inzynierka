using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler, IDropHandler, IBeginDragHandler,
    IPointerDownHandler, IPointerUpHandler
{
    public ItemSO itemSO;
    public int quantity;
    public Image itemImage;
    public TMP_Text quantityText;

    //public Image WholeSlot;


    private InventoryManager inventoryManager;
    private InventoryInfo inventoryInfo;
    private static ShopManager activeShop;

    /*dla przenoszenia itemow:*/
    private static Image dragIcon;
    private static Canvas rootCanvas;
    private static InventorySlot draggedSlot;

    private static bool isHoldingItem;

    [Header("Hotkey")]
    public KeyCode hotKey;


    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
        inventoryInfo = FindObjectOfType<InventoryInfo>();

        //ruszanie itemow
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

    }

    private void Update()
    {
        // ŒledŸ mysz podczas hover
        if (itemSO != null && inventoryInfo != null && inventoryInfo.infoPanel.alpha > 0)
        {
            inventoryInfo.FollowMouse();
        }

        if (hotKey != KeyCode.None && Input.GetKeyDown(hotKey))
        {
            UseFromHotkey();
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

        if (isHoldingItem || draggedSlot != null)//kiedy ruszamy item to nie pokazuj info
            return;

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

    private void UseFromHotkey()
    {
        if (itemSO == null || quantity <= 0)
            return;

        // Nie u¿ywamy collectible
        if (itemSO.itemType == ItemType.Collectible)
            return;

        // Equipment
        if (IsEquipment(itemSO.itemType))
        {
            if (EquipmentManager.Instance != null &&
                EquipmentManager.Instance.EquipItem(itemSO))
            {
                quantity--;
                if (quantity <= 0)
                    itemSO = null;

                UpdateUI();
                inventoryManager.SyncSlotsPublic(this);
            }
        }
        else
        {
            inventoryManager.UseItem(this);
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
                        if(itemSO.itemType == ItemType.Collectible)
                            return;

                        else
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemSO == null)
            return;

        draggedSlot = this;

        if (inventoryInfo != null)//jak przytrzymujemy to chowamy info
            inventoryInfo.HideItemInfo();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        if (dragIcon == null)
        {
            GameObject go = new GameObject("DragIcon");
            go.transform.SetParent(rootCanvas.transform, false);
            dragIcon = go.AddComponent<Image>();
            dragIcon.raycastTarget = false;
        }

        dragIcon.sprite = itemImage.sprite;
        dragIcon.enabled = true;
        dragIcon.transform.position = eventData.position;
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon == null)
            return;

        dragIcon.transform.position = eventData.position;
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this)
            return;

        SwapOrMove(draggedSlot, this);
        inventoryInfo.ShowItemInfo(itemSO);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.enabled = false;

        draggedSlot = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (itemSO == null)
            return;

        isHoldingItem = true;

        if (inventoryInfo != null)
            inventoryInfo.HideItemInfo();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHoldingItem = false;
    }


    private void SwapOrMove(InventorySlot from, InventorySlot to)
    {
        if (to.itemSO == null)
        {
            // przeniesienie
            to.itemSO = from.itemSO;
            to.quantity = from.quantity;

            from.itemSO = null;
            from.quantity = 0;
        }
        else
        {
            // zamiana
            ItemSO tempItem = to.itemSO;
            int tempQty = to.quantity;

            to.itemSO = from.itemSO;
            to.quantity = from.quantity;

            from.itemSO = tempItem;
            from.quantity = tempQty;
        }

        from.UpdateUI();
        to.UpdateUI();

        inventoryManager.SyncSlotsPublic(from);
        inventoryManager.SyncSlotsPublic(to);
    }


    private bool IsEquipment(ItemType type)
    {
        return type == ItemType.Ring ||
               type == ItemType.Weapon ||
               type == ItemType.Helmet ||
               type == ItemType.Chestplate ||
               type == ItemType.Boots ||
               type == ItemType.Legs;
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
            if (IsEquipment(itemSO.itemType) || itemSO.itemType == ItemType.Collectible)
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