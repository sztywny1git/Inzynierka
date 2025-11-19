using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public AbstractDungeonGenerator abstractDungeonGenerator;
    [SerializeField]
    public GameObject playerPrefab;
    [SerializeField]
    public GameObject enemyPrefab;
    [SerializeField]
    private GameObject player;

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        abstractDungeonGenerator.GenerateDungeon();

        //Vector2 playerSpawn = abstractDungeonGenerator.FindCentersOfRooms();
        //player = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);

        //List<Vector2> enemySpawns = abstractDungeonGenerator.GetEnemySpawnPoints(3);
        //foreach (Vector2 pos in enemySpawns)
        //{
        //    Instantiate(enemyPrefab, pos, Quaternion.identity);
        //}
    }

    public void OnPlayerDeath()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void OnWin()
    {
        // SceneManager.LoadScene("WinScene");
    }
}
