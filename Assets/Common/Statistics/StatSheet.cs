using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Stat Sheet", menuName = "Stats/Stat Sheet")]
public class StatSheet : ScriptableObject
{
    [System.Serializable]
    public class StatEntry
    {
        public StatDefinition Definition;
        public float BaseValue;
    }

    public List<StatEntry> Stats;
}