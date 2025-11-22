using UnityEngine;
using static Unity.VisualScripting.Member;

public class SkillManager : MonoBehaviour
{
    private PlayerStats playerStats;
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
        string skillName = slot.skillSO.skillName;
        string source = $"Skill_{skillName}_{slot.currentLevel}";

        switch (skillName)
        {
            case "Speed":
                playerStats.MoveSpeed.AddModifier(
                    new StatModifier(2f, true, source)
                );
                break;

            default:
                Debug.LogWarning("Nieznana umiejêtnoœæ: " + skillName);
                break;
        }

    }


}

