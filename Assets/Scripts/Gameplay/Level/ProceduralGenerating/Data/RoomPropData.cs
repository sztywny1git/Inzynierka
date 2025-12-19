using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RoomPropData", menuName = "Dungeon/Room Prop Data")]
public class RoomPropData : ScriptableObject
{
    // Nowa struktura do przechowywania Prefabów propów i ich ustawień
    [System.Serializable]
    public class PropEntry
    {
        public GameObject propPrefab;
        [Tooltip("Wielkość przesunięcia względem środka kafelka (np. 0.5 dla losowego przesunięcia).")]
        public float randomOffsetRange = 0.25f; 
    }

    [Header("Floor Props")]
    public List<PropEntry> floorProps;
    public int floorPropDensity = 5;

    [Header("Wall Props")]
    public List<PropEntry> wallProps;
    public int wallPropDensity = 3;
}
