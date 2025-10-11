using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UseItem : MonoBehaviour
{
    public void ApplyItemEffects(ItemSO itemSO)
    {
        if (itemSO.speed > 0)
            StatsManager.Instance.UpdateMaxSpeed(itemSO.speed);

        if (itemSO.duration > 0)
            StartCoroutine(EffectTimer(itemSO, itemSO.duration));

    }

    private IEnumerator EffectTimer(ItemSO itemSO, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (itemSO.speed > 0)
            StatsManager.Instance.UpdateMaxSpeed(-itemSO.speed);
    }
}
