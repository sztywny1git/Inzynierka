using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IEndDragHandler, IDropHandler, IBeginDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public ItemSO itemSO;
    public int quantity;
    public Image itemImage;
    public TMP_Text quantityText;

    private InventoryInfo inventoryInfo;
    private static ShopManager activeShop;

    private static Image dragIcon;
    private static Canvas rootCanvas;
    private static InventorySlot draggedSlot;
    private static bool isHoldingItem;

    [Header("Hotkey")]
    public KeyCode hotKey;

    private void Start()
    {
        inventoryInfo = FindFirstObjectByType<InventoryInfo>();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
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
        if (isHoldingItem || draggedSlot != null)
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
        if (itemSO == null || quantity <= 0) return;
        if (itemSO.itemType == ItemType.Collectible) return;

        if (IsEquipment(itemSO.itemType))
        {
            if (EquipmentManager.Instance != null && EquipmentManager.Instance.EquipItem(itemSO))
            {
                quantity--;
                if (quantity <= 0) itemSO = null;

                UpdateUI();
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.SyncSlotsPublic(this);
            }
        }
        else
        {
            if (InventoryManager.Instance != null)
                InventoryManager.Instance.UseItem(this);
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
                    if (InventoryManager.Instance != null)
                        InventoryManager.Instance.SyncSlotsPublic(this);
                }
                else
                {
                    if (IsEquipment(itemSO.itemType))
                    {
                        if (EquipmentManager.Instance != null && EquipmentManager.Instance.EquipItem(itemSO))
                        {
                            quantity--;
                            if (quantity <= 0) itemSO = null;
                            UpdateUI();
                            if (InventoryManager.Instance != null)
                                InventoryManager.Instance.SyncSlotsPublic(this);
                        }
                    }
                    else
                    {   
                        if(itemSO.itemType == ItemType.Collectible) return;
                        if (InventoryManager.Instance != null)
                            InventoryManager.Instance.UseItem(this);
                    }
                }
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (InventoryManager.Instance != null)
                    InventoryManager.Instance.DropItem(this);
            }
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (itemSO == null) return;

        draggedSlot = this;

        if (inventoryInfo != null)
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
        if (dragIcon == null) return;
        dragIcon.transform.position = eventData.position;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;

        SwapOrMove(draggedSlot, this);
        if(inventoryInfo != null) inventoryInfo.ShowItemInfo(itemSO);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.enabled = false;

        draggedSlot = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (itemSO == null) return;

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
            to.itemSO = from.itemSO;
            to.quantity = from.quantity;
            from.itemSO = null;
            from.quantity = 0;
        }
        else
        {
            ItemSO tempItem = to.itemSO;
            int tempQty = to.quantity;
            to.itemSO = from.itemSO;
            to.quantity = from.quantity;
            from.itemSO = tempItem;
            from.quantity = tempQty;
        }

        from.UpdateUI();
        to.UpdateUI();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SyncSlotsPublic(from);
            InventoryManager.Instance.SyncSlotsPublic(to);
        }
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

    public void UpdateUI()
    {
        if (quantity <= 0)
            itemSO = null;

        if (itemSO != null)
        {
            itemImage.sprite = itemSO.icon;
            itemImage.gameObject.SetActive(true);

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