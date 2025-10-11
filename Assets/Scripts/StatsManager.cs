using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance;
    public StatsUI statsUI;

    [Header("Movement Stats")]
    public float moveSpeed = 2f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float maxSpeed = 8f;

    [Header("Combat Stats")]
    public float fireRate = 8f;

    [Header("Bullet Stats")]
    public float bulletSpeed = 10f;
    public float bulletLifetime = 3f;
    public int bulletDamage = 1;

    [Header("Health Stats")]
    public int maxHearts = 6;
    public int currentHearts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateMaxSpeed(float amount)
    {
        moveSpeed = moveSpeed + amount;
        statsUI.UpdateAllStats();
    }
}
