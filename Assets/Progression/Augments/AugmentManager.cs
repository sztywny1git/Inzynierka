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

    private int _pendingLevelUps = 0;

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

    public void ResetAugments()
    {
        if (currentMediator != null)
        {
            foreach (var augment in activeAugments.Keys)
            {
                currentMediator.HandleAugment(augment, 0);
            }
        }

        activeAugments.Clear();
        _pendingLevelUps = 0;

        if (activeAugmentsUI != null)
        {
            activeAugmentsUI.RefreshUI();
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
        _pendingLevelUps += points;
        Debug.Log($"[AugmentManager] Level Up received! Pending: {_pendingLevelUps}");

        if (!isAugmentSelectionOpen)
        {
            TryShowNextSelectionOrResume();
        }
    }

    private void TryShowNextSelectionOrResume()
    {
        bool success = ShowNextAugmentSelection();
        
        if (!success)
        {
            ResumeGame();
        }
    }

    private bool ShowNextAugmentSelection()
    {
        List<AugmentSO> offeredAugments = GetRandomAugments(3);

        if (offeredAugments.Count == 0)
        {
            return false;
        }

        Time.timeScale = 0f;
        isAugmentSelectionOpen = true;
        
        if (audioSource != null && augmentGetOpenSound != null)
        {
            audioSource.PlayOneShot(augmentGetOpenSound);
        }
        
        // upewniamy sie czy panel wlaczony
        if (selectionUI != null)
        {
            selectionUI.gameObject.SetActive(true);
            selectionUI.ShowAugmentSelection(offeredAugments);
        }
        
        return true;
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

        _pendingLevelUps--;
        Debug.Log($"[AugmentManager] Augment selected. Remaining pending: {_pendingLevelUps}");

        if (_pendingLevelUps > 0)
        {
            TryShowNextSelectionOrResume();
        }
        else
        {
            ResumeGame();
        }
    }

    private void ResumeGame()
    {
        _pendingLevelUps = 0;
        Time.timeScale = 1f;
        isAugmentSelectionOpen = false;
        
        if (selectionUI != null)
        {
            selectionUI.gameObject.SetActive(false);
        }
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