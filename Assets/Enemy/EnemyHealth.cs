using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 50f;
    private float currentHealth;
    private Animator anim;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Goblin took " + amount + " damage. Current health: " + currentHealth);

        if (anim != null)
        {
            anim.SetTrigger("hit");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        if (anim != null)
        {
            anim.SetTrigger("die");
        }

        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        Destroy(gameObject, 2f);
    }
}