using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int health = 3;

    public Transform hitbox;
    public float hitboxRadius;
    public LayerMask damageSource;  
    private bool isHit;

    private bool hitCooldown = false;
    public Rigidbody2D rb;

    public float knockBackForce = 10;
    public float knockBackForceUp = 2;
    public ParticleSystem hitParticle; 

    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;


    void Update()
    {
        if (health >= 3)
        {
            heart1.SetActive(true);
            heart2.SetActive(true);
            heart3.SetActive(true);
        }
        else if (health == 2) 
        {
            heart1.SetActive(true);
            heart2.SetActive(true);
            heart3.SetActive(false);
        }
        else if (health == 1) 
        {
            heart1.SetActive(true);
            heart2.SetActive(false);
            heart3.SetActive(false);
        }
        else if (health <= 0)
        {
            heart1.SetActive(false);
            heart2.SetActive(false);
            heart3.SetActive(false);
        }
    }
}
