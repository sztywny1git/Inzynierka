using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    [Header("Augment Pool")]
    public List<AugmentSO> allAugments = new List<AugmentSO>();

    [Header("UI Reference")]
    public AugmentSelectionUI selectionUI;
    public ActiveAugmentsUI activeAugmentsUI;

    public static bool isAugmentSelectionOpen;

    [SerializeField] private PlayerStats playerStats;

    // S³ownik przechowuj¹cy aktywne augmenty gracza
    private Dictionary<AugmentSO, int> activeAugments = new Dictionary<AugmentSO, int>();

    // Event do powiadamiania o zmianach
    public delegate void AugmentChanged(AugmentSO augment, int stacks);
    public static event AugmentChanged OnAugmentAdded;

    public AudioSource audioSource;
    public AudioClip augmentGetOpenSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        ExpManager.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        ExpManager.OnLevelUp -= HandleLevelUp;
    }

    private void HandleLevelUp(int points)
    {
        // Zatrzymaj czas i poka¿ okienko wyboru
        Time.timeScale = 0f;

        isAugmentSelectionOpen = true;
        // Wybierz 3 losowe augmenty
        List<AugmentSO> offeredAugments = GetRandomAugments(3);

        audioSource.PlayOneShot(augmentGetOpenSound);

        // Poka¿ UI
        selectionUI.ShowAugmentSelection(offeredAugments);
    }

    private List<AugmentSO> GetRandomAugments(int count)
    {
        List<AugmentSO> available = new List<AugmentSO>(allAugments);
        List<AugmentSO> selected = new List<AugmentSO>();

        // Usuñ augmenty które osi¹gnê³y max stacki
        available.RemoveAll(aug =>
        {
            if (activeAugments.ContainsKey(aug))
            {
                return activeAugments[aug] >= aug.maxStacks;
            }
            return false;
        });

        // Jeœli mamy mniej ni¿ count dostêpnych augmentów, u¿yj wszystkich
        count = Mathf.Min(count, available.Count);

        // Wybierz losowe augmenty
        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            selected.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }

        return selected;
    }

    public void SelectAugment(AugmentSO augment)
    {
        if (activeAugments.ContainsKey(augment))
        {
            activeAugments[augment]++;
        }
        else
        {
            activeAugments.Add(augment, 1);
        }

        // Zastosuj efekt augmentu
        ApplyAugmentEffect(augment);

        Debug.Log($"Wybrano augment: {augment.augmentName} (Stack: {activeAugments[augment]})");

        OnAugmentAdded?.Invoke(augment, activeAugments[augment]);
        if (activeAugmentsUI != null)
        {
            activeAugmentsUI.RefreshUI();
        }

        // Wznów grê
        Time.timeScale = 1f;

        isAugmentSelectionOpen = false;
    }

    private void ApplyAugmentEffect(AugmentSO augment)
    {
        // Tutaj dodaj logikê aplikowania efektów
        // Mo¿esz u¿yæ eventów lub bezpoœrednio modyfikowaæ statystyki gracza
        switch (augment.augmentType)
        {
            case AugmentType.HealthBoost:
                    playerStats.Health.AddModifier(
                        new StatModifier(10, true, "Augment Health Boost")
                    );
                
                // Zwiêksz maksymalne HP
                // PlayerStats.Instance.IncreaseMaxHealth(augment.value);
                break;
            case AugmentType.DamageBoost:
                playerStats.Damage.AddModifier(
                        new StatModifier(3, true, "Augment Damage Boost")
                    );

                // Zwiêksz damage
                // PlayerStats.Instance.IncreaseDamage(augment.value);
                break;
            case AugmentType.SpeedBoost:
                playerStats.MoveSpeed.AddModifier(
                        new StatModifier(0.5f, true, "Augment Speed Boost")
                    );
                // Zwiêksz prêdkoœæ
                // PlayerStats.Instance.IncreaseSpeed(augment.value);
                break;
            // Dodaj pozosta³e case'y
            case AugmentType.CriticalChance:
                playerStats.CriticalChance.AddModifier(
                    new StatModifier(0.1f, true, "Critical Chance Boost")
                );
                break;
            case AugmentType.ArmorBoost:
                playerStats.Armor.AddModifier(
                    new StatModifier(5, true, "Armor Boost")
                );
                break;
            case AugmentType.ResourceBoost:
                playerStats.Resource.AddModifier(
                    new StatModifier(15, true, "Resource Boost")
                );
                break;
            case AugmentType.AttackSpeedBoost:
                playerStats.AttackSpeed.AddModifier(
                    new StatModifier(0.3f, true, "Attack Speed Boost")
                );
                break;

        }
    }

    public int GetAugmentStacks(AugmentSO augment)
    {
        return activeAugments.ContainsKey(augment) ? activeAugments[augment] : 0;
    }

    public Dictionary<AugmentSO, int> GetActiveAugments()
    {
        return new Dictionary<AugmentSO, int>(activeAugments);
    }
}