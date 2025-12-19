using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BossRoomData
{
    // Prefab bossa
    public GameObject bossPrefab;

    // Lista prefabów obiektów dekoracyjnych/rekwizytów (props)
    public List<GameObject> propPrefabs;

    // Opcjonalnie: Prefab obiektu teleportu powrotnego
    public GameObject exitTeleportPrefab;

    // Opcjonalnie: Warstwa, na której ma się znaleźć boss (np. LayerMask)
    // public LayerMask spawnLayer; 
}