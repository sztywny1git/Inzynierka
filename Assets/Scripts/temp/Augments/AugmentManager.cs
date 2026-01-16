using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : MonoBehaviour
{
    public static AugmentManager Instance { get; private set; }

    [Header("Augment Pool")]
    public List<AugmentSO> allAugments = new List<AugmentSO>();

    [Header("UI Reference")]
    public AugmentSelectionUI selectionUI;
    public ActiveAugmentsUI activeAugmentsUI;

    public static bool isAugmentSelectionOpen;

    private Dictionary<AugmentSO, int> activeAugments = new Dictionary<AugmentSO, int>();
    private StatMediator currentMediator;

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

    public void RegisterPlayerMediator(StatMediator mediator)
    {
        currentMediator = mediator;
        ReapplyAllAugments();
    }

    public void UnregisterPlayerMediator(StatMediator mediator)
    {
        if (currentMediator == mediator)
        {
            currentMediator = null;
        }
    }

    private void ReapplyAllAugments()
    {
        if (currentMediator == null) return;

        foreach (var kvp in activeAugments)
        {
            currentMediator.HandleAugment(kvp.Key, kvp.Value);
        }
    }

    private void HandleLevelUp(int points)
    {
        Time.timeScale = 0f;
        isAugmentSelectionOpen = true;
        List<AugmentSO> offeredAugments = GetRandomAugments(3);
        audioSource.PlayOneShot(augmentGetOpenSound);
        selectionUI.ShowAugmentSelection(offeredAugments);
    }

    private List<AugmentSO> GetRandomAugments(int count)
    {
        List<AugmentSO> available = new List<AugmentSO>(allAugments);
        List<AugmentSO> selected = new List<AugmentSO>();

        available.RemoveAll(aug =>
        {
            if (activeAugments.ContainsKey(aug))
            {
                return activeAugments[aug] >= aug.maxStacks;
            }
            return false;
        });

        count = Mathf.Min(count, available.Count);

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

        ApplyAugmentEffect(augment);

        OnAugmentAdded?.Invoke(augment, activeAugments[augment]);
        if (activeAugmentsUI != null)
        {
            activeAugmentsUI.RefreshUI();
        }

        Time.timeScale = 1f;
        isAugmentSelectionOpen = false;
    }

    private void ApplyAugmentEffect(AugmentSO augment)
    {
        if (currentMediator != null && activeAugments.ContainsKey(augment))
        {
            currentMediator.HandleAugment(augment, activeAugments[augment]);
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