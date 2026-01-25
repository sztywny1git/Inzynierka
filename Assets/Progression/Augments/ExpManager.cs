using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using VContainer;

public class ExpManager : MonoBehaviour
{
    public int level = 1;
    public int currentExp;
    public int expToNextLevel = 50;
    public float expGrowthFactor = 1.15f;
    public Slider expSlider;
    public TMP_Text currentLevelText;

    public static event Action<int> OnLevelUp; 

    private GameplayEventBus _eventBus;

    [Inject]
    public void Construct(GameplayEventBus eventBus)
    {
        _eventBus = eventBus;
    }

    private void Start()
    {
        if (_eventBus != null)
        {
            _eventBus.OnEnemyDied += HandleEnemyDied;
        }
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (_eventBus != null)
        {
            _eventBus.OnEnemyDied -= HandleEnemyDied;
        }
    }


    private void HandleEnemyDied(Vector3 position, LootTableSO loot, int amount)
    {
        GainExperience(amount);
    }

    public void GainExperience(int amount)
    {
        currentExp += amount;
        
        while(currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        
        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= expToNextLevel;
        level++;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expGrowthFactor);
        OnLevelUp?.Invoke(1);
    }

    public void UpdateUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = expToNextLevel;
            expSlider.value = currentExp;
        }
        
        if (currentLevelText != null)
        {
            currentLevelText.text = "Level " + level;
        }
    }
}