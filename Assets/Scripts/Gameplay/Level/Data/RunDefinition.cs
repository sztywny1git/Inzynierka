using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Run Definition", menuName = "Gameplay/Run Definition")]
public class RunDefinition : ScriptableObject
{
    public List<LevelDefinition> LevelSequence;
}