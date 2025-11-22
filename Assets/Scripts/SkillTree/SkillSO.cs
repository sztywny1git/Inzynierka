using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill Tree/Skill")]
public class SkillSO : ScriptableObject
{
    public string skillName;
    public int maxLevel;
    public Sprite icon;
}
