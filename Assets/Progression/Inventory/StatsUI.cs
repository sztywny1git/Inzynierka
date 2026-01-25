using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class StatsUI : MonoBehaviour
{
    public static StatsUI Instance;

    [Header("Stat Definitions Reference")]
    public StatDefinition DamageDef;
    public StatDefinition ArmorDef;
    public StatDefinition MoveSpeedDef;
    public StatDefinition MaxHealthDef;
    public StatDefinition MaxResourceDef;
    public StatDefinition AttackSpeedDef;
    public StatDefinition CritChanceDef;
    public StatDefinition CritDamageDef;

    [Header("References")]
    private CharacterStats currentStats;

    public GameObject[] statsSlots;
    public CanvasGroup statsCanvas;
    private bool statsOpen = true;

    // lista pomocnicza do zarzadzania eventami
    private List<StatDefinition> allDefinitions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        allDefinitions = new List<StatDefinition>
        {
            DamageDef, ArmorDef, MoveSpeedDef, MaxHealthDef, 
            MaxResourceDef, AttackSpeedDef, CritChanceDef, CritDamageDef
        };
    }

    public void RegisterPlayerStats(CharacterStats newStats)
    {
        if (currentStats != null)
        {
            currentStats.OnStatsReinitialized -= UpdateAllStats;
            UnsubscribeFromStatEvents();
        }

        currentStats = newStats;

        if (currentStats != null)
        {
            currentStats.OnStatsReinitialized += UpdateAllStats;
            
            SubscribeToStatEvents();

            UpdateAllStats();
        }
    }

    // pomocnicza metoda, zapisuje UI do nasluchiwania kazdej statystyki
    private void SubscribeToStatEvents()
    {
        foreach (var def in allDefinitions)
        {
            if (def == null) continue;
            
            var stat = currentStats.GetStat(def);
            if (stat != null)
            {
                // Gdy statystyka sie zmieni wywolaj metode
                stat.OnStatChanged += OnSingleStatChanged;
            }
        }
    }

    // wypisuje UI z nasluchiwania kazdej statystyki
    private void UnsubscribeFromStatEvents()
    {
        foreach (var def in allDefinitions)
        {
            if (def == null) continue;

            var stat = currentStats.GetStat(def);
            if (stat != null)
            {
                stat.OnStatChanged -= OnSingleStatChanged;
            }
        }
    }

    // odswiezamy wszystko
    private void OnSingleStatChanged(float newValue)
    {
        UpdateAllStats();
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (statsOpen)
            {
                statsCanvas.alpha = 0;
                statsCanvas.blocksRaycasts = false;
                statsOpen = false;
            }
            else
            {
                statsCanvas.alpha = 1;
                statsCanvas.blocksRaycasts = true;
                statsOpen = true;
                UpdateAllStats(); 
            }
        }
    }*/

    private void UpdateStatUI(int slotIndex, string label, StatDefinition def)
    {
        if (currentStats == null || def == null) return;
        
        float val = currentStats.GetFinalStatValue(def);
        UpdateStatText(slotIndex, label, val);
    }

    private void UpdateStatText(int slotIndex, string statName, float statValue)
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
        if (currentStats == null) return;

        UpdateStatUI(0, "MaxHealth", MaxHealthDef);
        UpdateStatUI(1, "Resource", MaxResourceDef);
        UpdateStatUI(2, "Damage", DamageDef);
        UpdateStatUI(3, "CritChance", CritChanceDef);
        UpdateStatUI(4, "CritDamage", CritDamageDef);
        UpdateStatUI(5, "AttackSpeed", AttackSpeedDef);
        UpdateStatUI(6, "Armor", ArmorDef);
        UpdateStatUI(7, "MoveSpeed", MoveSpeedDef);
    }
}