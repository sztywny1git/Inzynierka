// Spawner.cs
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    [SerializeField]
    private GameObject bossPrefab;

    [SerializeField]
    private float entityZPosition = -1f; 

    private GameObject currentPlayer;
    private GameObject currentBoss;

    public void SpawnPlayer(Vector2Int position)
    {
        // Remove existing player if any
        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        // Spawn new player with adjusted Z-position
        Vector3 spawnPosition = new Vector3(position.x, position.y, entityZPosition);
        currentPlayer = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Player spawned at: {spawnPosition}");
    }

    public void SpawnBoss(Vector2Int position)
    {
        // Remove existing boss if any
        if (currentBoss != null)
        {
            Destroy(currentBoss);
        }

        // Spawn new boss with adjusted Z-position
        Vector3 spawnPosition = new Vector3(position.x, position.y, entityZPosition);
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Boss spawned at: {spawnPosition}");
    }

    public void ClearSpawns()
    {
        if (currentPlayer != null) Destroy(currentPlayer);
        if (currentBoss != null) Destroy(currentBoss);
    }
}