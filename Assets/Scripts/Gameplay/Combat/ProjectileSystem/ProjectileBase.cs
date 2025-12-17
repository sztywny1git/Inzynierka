using UnityEngine;

public class ProjectileBase : MonoBehaviour
{
    private IProjectileMovementStrategy movementStrategy;
    private int pierceCount;
    private float damage;
    private int hits = 0;
    private Vector3 lastPosition;
    public GameObject Owner;
    
    public Vector3 MoveDirection { get; private set; }

    public void Initialize(GameObject owner, IProjectileMovementStrategy strategy, float damage, int pierceCount)
    {
        this.movementStrategy = strategy;
        this.damage = damage;
        this.pierceCount = pierceCount;
        this.Owner = owner;
        
        this.movementStrategy.Initialize(this);
    }
    
    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (movementStrategy != null)
        {
            movementStrategy.Move();
            
            if (Time.deltaTime > 0)
            {
                MoveDirection = (transform.position - lastPosition).normalized;
                lastPosition = transform.position;
            }

            if (movementStrategy.IsMovementDone)
            {
                HandleEndOfLife();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // TODO: Dodaj logikę sprawdzania tagu/warstwy wroga
        Debug.Log("Trafiono w: " + other.name + ", zadając " + damage + " obrażeń.");
        
        hits++;
        
        if (hits > pierceCount)
        {
            HandleEndOfLife();
        }
    }

    private void HandleEndOfLife()
    {
        // TODO: Dodaj logikę wybuchu
        Destroy(gameObject);
    }
}