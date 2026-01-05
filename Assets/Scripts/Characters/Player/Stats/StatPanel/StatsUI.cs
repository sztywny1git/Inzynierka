using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    [Header("References")]
    public PlayerStats playerStats;

    public GameObject[] statsSlots;
    public CanvasGroup statsCanvas;
    private bool statsOpen = true;

    private void Start()
    {
        // Znajdü PlayerStats jeúli nie jest przypisany
        if (playerStats == null)
        {
            playerStats = FindObjectOfType<PlayerStats>();
        }

        UpdateAllStats();


        if (playerStats != null)
        {
            playerStats.Health.OnStatChanged += (value) => UpdateStat(0, "MaxHealth", value);
            playerStats.Resource.OnStatChanged += (value) => UpdateStat(1, "Resource", value);
            playerStats.Damage.OnStatChanged += (value) => UpdateStat(2, "Damage", value);
            playerStats.CriticalChance.OnStatChanged += (value) => UpdateStat(3, "CritChance", value);
            playerStats.CriticalDamage.OnStatChanged += (value) => UpdateStat(4, "CritDamage", value);
            playerStats.AttackSpeed.OnStatChanged += (value) => UpdateStat(5, "AttackSpeed", value);
            playerStats.Armor.OnStatChanged += (value) => UpdateStat(6, "Armor", value);
            playerStats.MoveSpeed.OnStatChanged += (value) => UpdateStat(7, "MoveSpeed", value);
            

            //playerStats.Health.OnStatChanged += (value) => UpdateStat(2, "Health", value);
            /*playerStats.OnHealthChangedEvent += (current, max) =>
            {
                UpdateStat(2, "Health", current);
            };*/


            // itd...
        }
    }

    private void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += OnStatsChanged;
    }

    private void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= OnStatsChanged;
    }

    private void Update()
    {
        if (Input.GetButtonDown("ToggleStats"))
        {
            if (statsOpen)
            {
                //Time.timeScale = 0; //pazuje gre
                statsCanvas.alpha = 0;
                statsCanvas.blocksRaycasts = false;
                statsOpen = false;
            }
            else
            {
                //Time.timeScale = 1; //odpauzuje gre
                statsCanvas.alpha = 1;
                statsCanvas.blocksRaycasts = true;
                statsOpen = true;
            }
        }
    }

    private void OnStatsChanged(SkillSlot slot)
    {
        UpdateAllStats();
    }

    private void UpdateStat(int slotIndex, string statName, float statValue)
    {
        if (statsSlots.Length > slotIndex && statsSlots[slotIndex] != null)
        {
            TMP_Text statText = statsSlots[slotIndex].GetComponentInChildren<TMP_Text>();
            if (statText != null)
            {
                statText.text = $"{statName}: {statValue:F2}";
            }
        }
    }

    public void UpdateAllStats()
    {
        if (playerStats == null) return;

        UpdateStat(0, "MaxHealth", playerStats.Health.FinalValue);
        UpdateStat(1, "Resource", playerStats.Resource.FinalValue);
        UpdateStat(2, "Damage", playerStats.Damage.FinalValue);
        UpdateStat(3, "CritChance", playerStats.CriticalChance.FinalValue);
        UpdateStat(4, "CritDamage", playerStats.CriticalDamage.FinalValue);
        UpdateStat(5, "AttackSpeed", playerStats.AttackSpeed.FinalValue);
        UpdateStat(6, "Armor", playerStats.Armor.FinalValue);//to mozna dac jako oddzielne pod hp na stale
        UpdateStat(7, "MoveSpeed", playerStats.MoveSpeed.FinalValue);
        
        

        //UpdateStat(2, "Health", playerStats.Health.FinalValue);


        // itd...
    }
}