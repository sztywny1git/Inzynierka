using UnityEngine;

public class SkillManager : MonoBehaviour
{
    void OnEnable()
    {
        SkillSlot.OnAbilityPointSpent += HandleAbilityPointsSpent;
    }

    void OnDisable()
    {
        SkillSlot.OnAbilityPointSpent -= HandleAbilityPointsSpent;
    }

    private void HandleAbilityPointsSpent(SkillSlot slot)
    {
        string skillName = slot.skillSO.skillName; ;

        switch (skillName)
        {
            case "Speed":
                StatsManager.Instance.UpdateMaxSpeed(2);
                break;

            default:
                Debug.LogWarning("Nieznana umiejêtnoœæ: " + skillName);
                break;
        }

    }


}

