using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "New Character Definition", menuName = "Gameplay/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    [Header("Identity")]
    public string characterName;
    public Sprite characterSprite;
    public CharacterType Type;
    
    [Header("Stats")]
    public StatSheet statSheet;
    public List<StatModifierConfig> statBonuses;

    [Header("Abilities")]
    [Tooltip("Index 0 is considered the Basic Attack")]
    public List<Ability> abilities;
}