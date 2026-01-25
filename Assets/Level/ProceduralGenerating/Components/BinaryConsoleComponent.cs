using UnityEngine;

public class BinaryConsoleComponent : MonoBehaviour
{
    [Header("Logika")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [SerializeField, HideInInspector] private int targetValue;
    private bool isSolved = false;
    private bool isPlayerInRange = false; 
    private PuzzleManager puzzleManager;
    private Spawner spawner;
    private BinaryPuzzleUI puzzleUI;

    public void Initialize(PuzzleData data, Spawner spawnerRef, bool isKey)
    {
        this.targetValue = data.TargetValue;
        this.spawner = spawnerRef;

        //Debug.Log($"[CONSOLE] Initialize wywołane. Otrzymano TargetValue: {data.TargetValue}");
    }

    private void Start()
    {
        puzzleUI = FindFirstObjectByType<BinaryPuzzleUI>(FindObjectsInactive.Include);

        if (puzzleUI == null)
        {
            //Debug.LogError("BRAK BinaryPuzzleUI NA SCENIE! Upewnij się, że dodałeś prefab UI do Canvasu.");
        }
    }

    private void Update()
    {
        if (isSolved) return;

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (puzzleUI != null)
            {
                puzzleUI.OpenPuzzle(this.targetValue, this);
            }
        }
    }

    public void NotifySolved()
    {
        if (isSolved) return;
        
        isSolved = true;
        Debug.Log("ZAGADKA ROZWIĄZANA!");

        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.color = Color.green;

        if (puzzleManager == null) puzzleManager = FindFirstObjectByType<PuzzleManager>();
        
        if (puzzleManager != null)
        {
            Vector2Int gridPos = new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));
            puzzleManager.SolveBinaryPuzzle(gridPos);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}