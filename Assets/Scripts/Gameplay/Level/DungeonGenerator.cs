using UnityEngine;

public class DungeonGenerator
{
    public LevelData Generate(LevelDefinition definition, int seed)
    {
        // Mock
        return new LevelData
        {
            PlayerSpawnPosition = Vector3.zero,
            NextLevelPortalPosition = new Vector3(5f, 0f, 0f),
            EndRunPortalPosition = new Vector3(8f, 0f, 0f)
        };
    }
}