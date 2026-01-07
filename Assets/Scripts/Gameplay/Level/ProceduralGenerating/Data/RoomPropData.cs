using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoomPropData", menuName = "Dungeon/Room Prop Data")]
public class RoomPropData : ScriptableObject
{
    [System.Serializable]
    public struct WfcWeightEntry
    {
        public PropWFC.PropType type;
        [Range(0, 200)] public int weight;
    }

    [Header("Old Random Walk Settings (Legacy)")]
    public List<PropEntry> floorProps; // Twoje stare pola...
    [Range(0, 100)] public int floorPropDensity = 50;

    [Header("WFC Settings (New)")]
    [Tooltip("Zdefiniuj szanse na pojawienie się obiektów w tym typie pokoju.")]
    public List<WfcWeightEntry> wfcPropsRules;

    // Struktura pomocnicza dla starego systemu (jeśli jej używasz)
    [System.Serializable]
    public struct PropEntry
    {
        public GameObject propPrefab;
        public float randomOffsetRange;
    }

    // Metoda pomocnicza zamieniająca listę z Inspektora na Słownik dla algorytmu
    public Dictionary<PropWFC.PropType, int> GetWfcWeights()
    {
        Dictionary<PropWFC.PropType, int> dict = new Dictionary<PropWFC.PropType, int>();

        // 1. Dodaj wagi zdefiniowane przez Ciebie w Inspektorze
        foreach(var entry in wfcPropsRules)
        {
            if (!dict.ContainsKey(entry.type))
            {
                dict.Add(entry.type, entry.weight);
            }
        }

        // Automatycznie uzupełnij brakujące typy wagą 0
        // Iterujemy po wszystkich wartościach z Enuma PropType
        foreach (PropWFC.PropType type in System.Enum.GetValues(typeof(PropWFC.PropType)))
        {
            if (!dict.ContainsKey(type))
            {
                dict.Add(type, 0);
            }
        }

        // 3. Zabezpieczenie: 'Empty' musi mieć wagę > 0, żeby algorytm zawsze miał wyjście awaryjne
        if (dict[PropWFC.PropType.Empty] == 0)
        {
            Debug.LogWarning($"Typ 'Empty' miał wagę 0 w {this.name}. Ustawiam domyślnie na 50.");
            dict[PropWFC.PropType.Empty] = 50;
        }

        return dict;
    }
}