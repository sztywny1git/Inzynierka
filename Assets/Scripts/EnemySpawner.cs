using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public List<GameObject> Enemies = new List<GameObject>();
    public float spawnRate;

    private float x;
    private float y;
    private Vector3 spawnPos;

    void Start()
    {
        StartCoroutine(SpawnTestEnemy());
    }

    IEnumerator SpawnTestEnemy()
    {
        x = Random.Range(-1, 1);
        y = Random.Range(-1, 1);
        spawnPos.x += x;
        spawnPos.y += y;
        Instantiate(Enemies[0],spawnPos, Quaternion.identity);  
        yield return new WaitForSeconds(spawnRate);
        StartCoroutine(SpawnTestEnemy());
        spawnPos.x= 0;
        spawnPos.y= 0;
    }
}
