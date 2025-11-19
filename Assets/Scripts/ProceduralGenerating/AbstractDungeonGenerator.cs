using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer = null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;
    [SerializeField]
    protected Spawner spawner;

    public void GenerateDungeon()
    {
        tilemapVisualizer.Clear();

        if (spawner != null)
        {
            spawner.ClearSpawns();
        }

        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}