using UnityEngine;

public static class DungeonHelper
{
    // Statyczna metoda do obliczania centrum pokoju
    public static Vector2Int GetRoomCenter(BoundsInt room)
    {
        Vector3 center3 = room.center;
        return new Vector2Int(Mathf.RoundToInt(center3.x), Mathf.RoundToInt(center3.y));
    }
}