using UnityEngine;

public interface IEnemyFactory
{
    GameObject CreateEnemy(EnemyType type, Vector3 at);
}