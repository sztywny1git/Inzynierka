using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO equippedItem;
    public Image itemImage;

    private EquipmentManager equipmentManager;

    private void Start()
    {
        equipmentManager = GetComponentInParent<EquipmentManager>();
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