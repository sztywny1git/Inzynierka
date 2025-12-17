using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Character Definition", menuName = "Gameplay/Character Definition")]
public class CharacterDefinition : ScriptableObject
{
    public string characterName;
    public Sprite characterSprite;
    
    [Header("Base Stats")]
    public StatSheet statSheet;

    [Header("Permanent Bonuses")]
    public List<StatModifierConfig> statBonuses;

    [Header("Abilities")]
    [Tooltip("Index 0 is considered the Basic Attack")]
    public List<Ability> abilities;
}