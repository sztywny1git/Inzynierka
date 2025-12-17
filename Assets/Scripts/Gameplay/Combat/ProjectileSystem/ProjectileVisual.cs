using UnityEngine;

public class ProjectileVisual : MonoBehaviour
{
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Transform shadowTransform;

    private ProjectileBase projectile;
    private Vector3 startPosition;

    public void Initialize(ProjectileBase projectile)
    {
        this.projectile = projectile;
    }
    
    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (projectile == null) return;

        UpdateRotation();
        UpdateShadowPosition();
    }
    
    private void UpdateRotation()
    {
        if (visualTransform == null) return;
        Vector3 moveDir = projectile.MoveDirection;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            visualTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void UpdateShadowPosition()
    {
        if (shadowTransform == null) return;
        Vector3 shadowPosition = transform.position;
        shadowPosition.y = startPosition.y; 
        shadowTransform.position = shadowPosition;
    }
}