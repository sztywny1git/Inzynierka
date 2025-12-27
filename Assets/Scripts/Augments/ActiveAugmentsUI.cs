using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActiveAugmentsUI : MonoBehaviour
{
    [Header("UI References")]
    public Image[] augmentIcons; // Wszystkie ikonki (np. 10 sztuk)

    private void Start()
    {
        // Na starcie ukryj wszystkie ikonki
        foreach (var icon in augmentIcons)
        {
            icon.gameObject.SetActive(false);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        // Pobierz wszystkie aktywne augmenty z managera
        Dictionary<AugmentSO, int> activeAugments = AugmentManager.Instance.GetActiveAugments();

        // Ukryj wszystkie ikonki najpierw
        foreach (var icon in augmentIcons)
        {
            icon.gameObject.SetActive(false);
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
            index++;
        }
    }
}