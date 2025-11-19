using UnityEngine;

public enum ProjectileOwner { Player, Enemy, Neutral }

public class ProjectileFactory : MonoBehaviour
{
    public static ProjectileFactory Instance { get; private set; }
    [SerializeField] private ProjectileBase defaultPrefab;

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public ProjectileBase Spawn(
        ProjectileBase prefab,
        Vector3 position,
        IProjectileMovementStrategy movementStrategy,
        ProjectileOwner owner,
        float damage,
        int pierceCount
    )
    {
        if (prefab == null) prefab = defaultPrefab;
        if (prefab == null) return null;

        var go = Instantiate(prefab.gameObject, position, Quaternion.identity);
        SetProjectileLayer(go, owner);
        
        var projectile = go.GetComponent<ProjectileBase>();
        if (projectile != null)
        {
            projectile.Initialize(movementStrategy, damage, pierceCount);
        }

        var visual = go.GetComponent<ProjectileVisual>();
        if (visual != null)
        {
            visual.Initialize(projectile);
        }

        return projectile;
    }
    
    private void SetProjectileLayer(GameObject projectileObject, ProjectileOwner owner)
    {
        string layerName = "";
        switch (owner)
        {
            case ProjectileOwner.Player: layerName = "PlayerProjectiles"; break;
            case ProjectileOwner.Enemy: layerName = "EnemyProjectiles"; break;
        }

        if (!string.IsNullOrEmpty(layerName))
        {
            int layer = LayerMask.NameToLayer(layerName);

            if (layer == -1)
            {
                Debug.LogWarning($"Layer '{layerName}' not found.");
            }
            else
            {
                projectileObject.layer = layer;
            }
        }
    }
}