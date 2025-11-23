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
            playerStats.MoveSpeed.OnStatChanged += (value) => UpdateStat(0, "MoveSpeed", value);
            playerStats.Armor.OnStatChanged += (value) => UpdateStat(1, "Armor", value);
            //playerStats.Health.OnStatChanged += (value) => UpdateStat(2, "Health", value);
            /*playerStats.OnHealthChangedEvent += (current, max) =>
            {
                UpdateStat(2, "Health", current);
            };*/
            playerStats.Damage.OnStatChanged += (value) => UpdateStat(3, "Damage", value);
            playerStats.AttackSpeed.OnStatChanged += (value) => UpdateStat(4, "AttackSpeed", value);
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

        UpdateStat(0, "MoveSpeed", playerStats.MoveSpeed.FinalValue);
        UpdateStat(1, "Armor", playerStats.Armor.FinalValue);
        //UpdateStat(2, "Health", playerStats.Health.FinalValue);
        UpdateStat(3, "Damage", playerStats.Damage.FinalValue);
        UpdateStat(3, "AttackSpeed", playerStats.AttackSpeed.FinalValue);
        // itd...
    }
}