using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class UseItem : MonoBehaviour
{
    private PlayerStats playerStats;
    public void ApplyItemEffects(ItemSO itemSO)
    {
        // Tylko dla consumables
        if (itemSO.itemType != ItemType.Consumable)
            return;

        string source = $"Consumable_{itemSO.name}_{Time.time}";

        if (itemSO.speed > 0)
            playerStats.MoveSpeed.AddModifier(
                    new StatModifier(itemSO.speed, true, source)
                );

        /*if (itemSO.currentHearts > 0)
            StatsManager.Instance.UpdateHealth(itemSO.currentHearts);

        if (itemSO.duration > 0)
            StartCoroutine(EffectTimer(itemSO, itemSO.duration));*/
    }

    private IEnumerator EffectTimer(ItemSO itemSO, float duration)
    {
        yield return new WaitForSeconds(duration);

        string source = $"Consumable_{itemSO.name}_{Time.time}";

        if (itemSO.speed > 0)
            playerStats.MoveSpeed.AddModifier(
                    new StatModifier(-itemSO.speed, true, source)
                );
    }
}