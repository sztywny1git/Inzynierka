using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartsUI : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartLost;

    [Header("Hearts Container")]
    [SerializeField] private GameObject heartPrefab;
    [SerializeField] private Transform heartsContainer;

    //[Header("UI References")]
    //[SerializeField] private Text hpText; // <- dodany tekst do wyœwietlania liczby HP

    private readonly List<Image> heartImages = new List<Image>();

    private void Start()
    {
        if (heartPrefab == null || heartsContainer == null)
        {
            Debug.LogError("HeartsUI: Przypisz Heart Prefab oraz Hearts Container!");
            return;
        }

        // Wyczyœæ wszystkie stare dzieci
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        heartImages.Clear();

        // Utwórz nowe serca
        for (int i = 0; i < StatsManager.Instance.maxHearts; i++)
        {
            GameObject heartInstance = Instantiate(heartPrefab, heartsContainer);

            // Szuka Image tak¿e w dzieciach, nie tylko na root prefabie
            Image heartImage = heartInstance.GetComponentInChildren<Image>();
            if (heartImage != null)
            {
                heartImages.Add(heartImage);
            }
            else
            {
                Debug.LogWarning("Heart prefab nie ma komponentu Image!");
            }
        }

        Refresh();
    }

    private void Update()
    {
        Refresh(); // co klatkê odœwie¿amy widok serc
    }

    /*public void SetHearts(int value)
    {
        currentHearts = Mathf.Clamp(value, 0, maxHearts);
        Refresh();
    }

    public void Heal(int amount = 1)
    {
        SetHearts(currentHearts + Mathf.Max(1, amount));
    }

    public void Damage(int amount = 1)
    {
        SetHearts(currentHearts - Mathf.Max(1, amount));
    }*/

    private void Refresh()
    {
        int currentHearts = StatsManager.Instance.currentHearts;
        int maxHearts = StatsManager.Instance.maxHearts;

        if (heartFull == null || heartLost == null)
        {
            Debug.LogWarning("HeartsUI: przypisz heartFull oraz heartLost");
            return;
        }

        for (int i = 0; i < heartImages.Count; i++)
        {
            if (i < currentHearts)
                heartImages[i].sprite = heartFull;
            else
                heartImages[i].sprite = heartLost;
        }

        // Update tekstu HP
        /*if (hpText != null)
        {
            hpText.text = currentHearts + " / " + maxHearts;
        }*/
    }
}
