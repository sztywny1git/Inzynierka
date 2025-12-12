using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public ItemSO equippedItem;
    public Image itemImage;

    private EquipmentManager equipmentManager;
    private InventoryInfo inventoryInfo;

    private void Start()
    {
        equipmentManager = GetComponentInParent<EquipmentManager>();
        inventoryInfo = FindObjectOfType<InventoryInfo>();
    }

    private void Update()
    {
        // ŒledŸ mysz podczas hover
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
        }
        else
        {
            itemImage.gameObject.SetActive(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && equippedItem != null)
        {
            equipmentManager.UnequipItem(this);
        }
    }
}