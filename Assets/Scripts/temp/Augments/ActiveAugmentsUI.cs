using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ActiveAugmentsUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] augmentIcons; // Wszystkie ikonki (np. 10 sztuk)
    public TMP_Text[] augmentStackTexts;

    private void Start()
    {
        // Na starcie ukryj wszystkie ikonki
        for (int i = 0; i < augmentIcons.Length; i++)
        {
            augmentIcons[i].gameObject.SetActive(false);
            augmentStackTexts[i].gameObject.SetActive(false);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        // Pobierz wszystkie aktywne augmenty z managera
        Dictionary<AugmentSO, int> activeAugments = AugmentManager.Instance.GetActiveAugments();

        // Ukryj wszystkie ikonki najpierw
        for (int i = 0; i < augmentIcons.Length; i++)
        {
            augmentIcons[i].gameObject.SetActive(false);
            augmentStackTexts[i].gameObject.SetActive(false);

            // Wyczyœæ augmentSO
            AugmentInfoSlot slot = augmentIcons[i].GetComponent<AugmentInfoSlot>();
            if (slot != null)
            {
                slot.augmentSO = null;
            }
        }

        // Wype³nij ikonki aktywnymi augmentami
        int index = 0;
        foreach (var kvp in activeAugments)
        {
            if (index >= augmentIcons.Length)
            {
                Debug.LogWarning("Za ma³o slotów dla wszystkich augmentów!");
                break;
            }

            augmentIcons[index].sprite = kvp.Key.icon;
            augmentIcons[index].gameObject.SetActive(true);

            AugmentInfoSlot slot = augmentIcons[index].GetComponent<AugmentInfoSlot>();
            if (slot != null)
            {
                slot.augmentSO = kvp.Key; 
                Debug.Log($"Przypisano {kvp.Key.augmentName} do slotu {index}");
            }

            int currentStacks = kvp.Value;
            int maxStacks = kvp.Key.maxStacks;

            int stacks = kvp.Value;

            if (stacks > 0)
            {
                augmentStackTexts[index].text = $"{currentStacks}/{maxStacks}";
                augmentStackTexts[index].gameObject.SetActive(true);
            }

            index++;
        }
    }
}