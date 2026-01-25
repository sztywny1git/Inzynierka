using UnityEngine;

[RequireComponent(typeof(IHealthProvider))]
public class DamageNumberSpawner : MonoBehaviour
{
    [SerializeField] private FloatingDamageText damageTextPrefab; // Zmiana typu na nasz skrypt
    [SerializeField] private Vector3 spawnOffset = new Vector3(0, 1.5f, 0);
    
    // Zwiększamy nieco zakres, żeby startowały z różnych miejsc
    [SerializeField] private Vector2 randomOffsetRange = new Vector2(0.5f, 0.3f); 

    private IHealthProvider _healthProvider;

    private void Awake()
    {
        _healthProvider = GetComponent<IHealthProvider>();
    }

    private void OnEnable()
    {
        if (_healthProvider != null)
            _healthProvider.OnDamageTaken += SpawnDamageText;
    }

    private void OnDisable()
    {
        if (_healthProvider != null)
            _healthProvider.OnDamageTaken -= SpawnDamageText;
    }

    private void SpawnDamageText(DamageData data)
    {
        if (damageTextPrefab == null) return;

        // 1. Losowa pozycja startowa (zapobiega idealnemu nakładaniu się na starcie)
        float randomX = Random.Range(-randomOffsetRange.x, randomOffsetRange.x);
        float randomY = Random.Range(-randomOffsetRange.y, randomOffsetRange.y);
        Vector3 finalPosition = transform.position + spawnOffset + new Vector3(randomX, randomY, 0);

        // 2. Instancjonowanie
        var instance = Instantiate(damageTextPrefab, finalPosition, Quaternion.identity);

        // 3. Konfiguracja (przekazujemy czy to krytyk)
        instance.Setup(data.Amount, data.IsCritical);
    }
}