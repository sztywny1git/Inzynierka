using Unity.VisualScripting;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    protected GameObject player;
    public virtual void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>().gameObject;
    }
}
