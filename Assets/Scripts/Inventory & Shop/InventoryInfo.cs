using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class InventoryInfo : MonoBehaviour
{
    public CanvasGroup infoPanel;

    public Image itemIcon;

    public TMP_Text itemNameText;

    public TMP_Text itemTypeText;

    public TMP_Text itemDescriptionText;

    public TMP_Text valueText;

    [Header("Stat Fields")]
    public TMP_Text[] statTexts;

    public TMP_Text rarityText;
    public Outline panelOutline;

    private RectTransform infoPanelRect;

    private void Awake()
    {
        infoPanelRect = GetComponent<RectTransform>();
    }

    public Color ColorFromHex(string hex)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hex, out color))
            return color;
        else
            return Color.white; // fallback jeœli hex niepoprawny
    }


    public void ApplyRarity(ItemSO item)
    {
        switch (item.rarity)
        {
            case Rarity.Common:
                rarityText.text = "Common";
                rarityText.color = Color.gray;
                panelOutline.effectColor = Color.gray;
                break;

            case Rarity.Rare:
                rarityText.text = "Rare";
                rarityText.color = ColorFromHex("#87CEFA");
                panelOutline.effectColor = ColorFromHex("#87CEFA");
                break;

            case Rarity.Epic:
                rarityText.text = "Epic";
                rarityText.color = ColorFromHex("#B026FF");
                panelOutline.effectColor = ColorFromHex("#B026FF");
                break;

            case Rarity.Legendary:
                rarityText.text = "Legendary";
                rarityText.color = new Color(1f, 0.6f, 0f); // z³oty
                panelOutline.effectColor = new Color(1f, 0.6f, 0f);
                break;
        }
    }




    public void ShowItemInfo(ItemSO itemSO)
    {
        infoPanel.alpha = 1;

        ApplyRarity(itemSO);

        itemIcon.sprite = itemSO.icon;
        itemIcon.enabled = itemSO.icon != null; // w³¹cz/wy³¹cz jeœli brak ikony

        itemNameText.text = itemSO.itemName;

        itemTypeText.text = itemSO.itemType.ToString();

        itemDescriptionText.text = itemSO.itemDescription;

        valueText.text = itemSO.value.ToString();

        List<string> stats = new List<string>();

        if (itemSO.currentHearts > 0)
            stats.Add("Hearts: " + itemSO.currentHearts.ToString());

        if (itemSO.speed > 0)
            stats.Add("Speed: " + itemSO.speed.ToString());

        if (itemSO.damage > 0)
            stats.Add("Damage: " + itemSO.damage.ToString());

        if (itemSO.damage > 0)
            stats.Add("AttackSpeed: " + itemSO.fireRate.ToString());

        if (itemSO.duration > 0)
            stats.Add("Duration: " + itemSO.duration.ToString());

        if (stats.Count <= 0)
        {
            foreach (var statText in statTexts)
            {
                statText.gameObject.SetActive(false);
            }
            return;
        }

        for (int i = 0; i < statTexts.Length; i++)
        {
            if (i < stats.Count)
            {
                statTexts[i].text = stats[i];
                statTexts[i].gameObject.SetActive(true);
            }
            else
            {
                statTexts[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideItemInfo()
    {
        infoPanel.alpha = 0;
        itemNameText.text = "";
        itemTypeText.text = "";
        itemDescriptionText.text = "";
        rarityText.text = "";
        valueText.text = "";

        itemIcon.sprite = null;
        itemIcon.enabled = false;

        foreach (var statText in statTexts)
        {
            statText.gameObject.SetActive(false);
        }
    }

    public void FollowMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = new Vector3(-120, -70, 0);
        Vector3 targetPosition = mousePosition + offset;

        float panelWidth = infoPanelRect.sizeDelta.x;
        float panelHeight = infoPanelRect.sizeDelta.y;

        // Pivot panelu
        float pivotX = infoPanelRect.pivot.x;
        float pivotY = infoPanelRect.pivot.y;

        // Lewa granica
        if (targetPosition.x - panelWidth * pivotX < 0)
        {
            targetPosition.x = panelWidth * pivotX;
        }

        // Prawa granica
        if (targetPosition.x + panelWidth * (1 - pivotX) > Screen.width)
        {
            targetPosition.x = Screen.width - panelWidth * (1 - pivotX);
        }

        // Dolna granica
        if (targetPosition.y - panelHeight * pivotY < 0)
        {
            targetPosition.y = panelHeight * pivotY;
        }

        // Górna granica
        if (targetPosition.y + panelHeight * (1 - pivotY) > Screen.height)
        {
            targetPosition.y = Screen.height - panelHeight * (1 - pivotY);
        }

        infoPanelRect.position = targetPosition;

    }
}