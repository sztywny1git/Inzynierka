using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class ShopInfo : MonoBehaviour
{
    public CanvasGroup infoPanel;

    public Image itemIcon;

    public TMP_Text itemTypeText;

    public TMP_Text rarityText;
    public Outline panelOutline;

    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    [Header ("Stat Fields")]
    public TMP_Text[] statTexts;

    private RectTransform infoPanelRect;

    private void Update()
    {
        if (infoPanel.alpha > 0)
            FollowMouse();
    }

    private void Awake()
    {
        infoPanelRect = GetComponent <RectTransform>();
        infoPanel.blocksRaycasts = false;
        infoPanel.interactable = false;
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
        itemIcon.enabled = itemSO.icon != null;

        itemNameText.text = itemSO.itemName;

        itemTypeText.text = itemSO.itemType.ToString();

        itemDescriptionText.text = itemSO.itemDescription;

        List<string> stats = new List<string>();
        if (itemSO.currentHearts != 0)
            stats.Add("Hearts: " + itemSO.currentHearts.ToString());

        if (itemSO.speed != 0)
            stats.Add("Speed: " + itemSO.speed.ToString());

        if (itemSO.damage != 0)
            stats.Add("Damage: " + itemSO.damage.ToString());

        if (itemSO.Resource != 0)
            stats.Add("Resource: " + itemSO.Resource.ToString());

        if (itemSO.armor != 0)
            stats.Add("Armor: " + itemSO.armor.ToString());

        if (itemSO.fireRate != 0)
            stats.Add("AttackSpeed: " + itemSO.fireRate.ToString());

        if (itemSO.CriticalChance != 0)
            stats.Add("CriticalChance: " + itemSO.CriticalChance.ToString());

        if (itemSO.CriticalDamage != 0)
            stats.Add("CriticalChance: " + itemSO.CriticalDamage.ToString());

        if (itemSO.duration != 0)
            stats.Add("Duration: " + itemSO.duration.ToString());

        if (stats.Count <= 0)
            return;

        for (int i = 0; i < statTexts.Length; i++)
        {
            if(i < stats.Count)
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

        itemIcon.sprite = null;
        itemIcon.enabled = false;
    }

    public void FollowMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = new Vector3(80, 100, 0);

        infoPanelRect.position = mousePosition + offset;
    }


}
