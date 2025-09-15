using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Transform attackPoint;
    public float weaponRange = 1;
    public float knockbackForce = 50;
    public float knockbackTime = 0.15f;
    public float stuntime = 0.3f;
    public LayerMask enemyLayers;
    public int damage = 1;


}
