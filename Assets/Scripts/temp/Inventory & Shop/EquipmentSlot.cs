using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemSO equippedItem;
    public Image itemImage;
    
    private InventoryInfo inventoryInfo;

    [Header("Slot background")]
    public Image backgroundImage;
    public Sprite emptyBackground;
    public Sprite equippedBackground;

    private void Start()
    {
        inventoryInfo = FindFirstObjectByType<InventoryInfo>();
    }

    private void Update()
    {
        if (equippedItem != null && inventoryInfo != null && inventoryInfo.infoPanel.alpha > 0)
        {
            inventoryInfo.FollowMouse();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedItem != null && inventoryInfo != null)
        {
            inventoryInfo.ShowItemInfo(equippedItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryInfo != null)
        {
            inventoryInfo.HideItemInfo();
        }
    }

    public void Equip(ItemSO itemSO)
    {
        equippedItem = itemSO;
        UpdateUI();
    }

    public void Unequip()
    {
        equippedItem = null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (equippedItem != null)
        {
            itemImage.sprite = equippedItem.icon;
            itemImage.gameObject.SetActive(true);

            if (backgroundImage != null && equippedBackground != null)
                backgroundImage.sprite = equippedBackground;
        }
        else
        {
            itemImage.gameObject.SetActive(false);

            if (backgroundImage != null && emptyBackground != null)
                backgroundImage.sprite = emptyBackground;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && equippedItem != null)
        {
            if (EquipmentManager.Instance != null)
            {
                EquipmentManager.Instance.UnequipItem(this);
            }
        }
    }
}