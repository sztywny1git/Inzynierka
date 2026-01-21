using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class ExpManager : MonoBehaviour
{
    public int level;
    public int currentExp;
    public int expToNextLevel = 5;
    public float expGrowthFactor = 1.3f;
    public Slider expSlider;
    public TMP_Text currentLevelText;

    public static event Action<int> OnLevelUp; 

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GainExperience(2); //Dodawanie expa dla testu
        }
    }

    /*private void OnEnable()
    {
        Enemy_Health.OnEnemyDefeated += GainExperience;
    }

    private void OnDisable()
    {
        Enemy_Health.OnEnemyDefeated -= GainExperience;
    }*/

    public void GainExperience(int amount)
    {
        currentExp += amount; ;
        if(currentExp >= expToNextLevel)
        {
            LevelUp();
        }
        UpdateUI();
    }

    private void LevelUp()
    {
        level++;
        currentExp -= expToNextLevel;
        expToNextLevel = Mathf.RoundToInt(expToNextLevel * expGrowthFactor);
        OnLevelUp?.Invoke(1);//Dostajemy 1pkt umj za lvl

    }


    public void UpdateUI()
    {
        expSlider.maxValue = expToNextLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Level " + level;
    }


}
