using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class AugmentSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject selectionPanel;
    public AugmentOptionUI[] augmentOptions; 

    private List<AugmentSO> currentOfferedAugments;

    private void Start()
    {
        selectionPanel.SetActive(false);

        for (int i = 0; i < augmentOptions.Length; i++)
        {
            int index = i; 
            augmentOptions[i].selectButton.onClick.AddListener(() => OnAugmentSelected(index));
        }
    }

    public void ShowAugmentSelection(List<AugmentSO> augments)
    {
        currentOfferedAugments = augments;

        selectionPanel.SetActive(true);

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
            AugmentManager.Instance.SelectAugment(selectedAugment);
            
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

        // Poka� informacj� o stackach je�li augment jest stackowalny
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