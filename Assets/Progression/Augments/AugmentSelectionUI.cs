using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AugmentSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectionPanel;
    public AugmentOptionUI[] augmentOptions; // 3 sloty

    private List<AugmentSO> currentOfferedAugments;

    private void Start()
    {
        // Upewnij siê, ¿e panel jest ukryty na starcie
        selectionPanel.SetActive(false);

        // Przypisz listenery do buttonów
        for (int i = 0; i < augmentOptions.Length; i++)
        {
            int index = i; // Zmienna lokalna dla closure
            augmentOptions[i].selectButton.onClick.AddListener(() => OnAugmentSelected(index));
        }
    }

    public void ShowAugmentSelection(List<AugmentSO> augments)
    {
        currentOfferedAugments = augments;

        // Wyœwietl panel
        selectionPanel.SetActive(true);

        // Wype³nij sloty augmentami
        for (int i = 0; i < augmentOptions.Length; i++)
        {
            if (i < augments.Count)
            {
                augmentOptions[i].Setup(augments[i]);
                augmentOptions[i].gameObject.SetActive(true);
            }
            else
            {
                augmentOptions[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnAugmentSelected(int index)
    {
        if (index < currentOfferedAugments.Count)
        {
            AugmentSO selectedAugment = currentOfferedAugments[index];

            // Powiadom AugmentManager o wyborze
            AugmentManager.Instance.SelectAugment(selectedAugment);

            // Ukryj panel
            selectionPanel.SetActive(false);
        }
    }
}

[System.Serializable]
public class AugmentOptionUI
{
    public GameObject gameObject;
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text stackText;
    public Button selectButton;

    public void Setup(AugmentSO augment)
    {
        iconImage.sprite = augment.icon;
        nameText.text = augment.augmentName;
        descriptionText.text = augment.description;

        // Poka¿ informacjê o stackach jeœli augment jest stackowalny
        if (augment.isStackable)
        {
            int currentStacks = AugmentManager.Instance.GetAugmentStacks(augment);
            stackText.text = $"{currentStacks}/{augment.maxStacks}";
            stackText.gameObject.SetActive(true);
        }
        else
        {
            stackText.gameObject.SetActive(false);
        }
    }
}