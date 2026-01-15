using UnityEngine;

public class PressurePlateComponent : MonoBehaviour
{
    // === Konfiguracja w Inspektorze ===
    [Header("Visuals")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite unpressedSprite; // Sprite dla stanu NIEWCIŚNIĘTY
    [SerializeField]
    private Sprite pressedSprite;   // Sprite dla stanu WCIŚNIĘTY
    
    [Header("Logic")]
    [Tooltip("Tag gracza lub obiektu, który ma aktywować płytkę.")]
    [SerializeField]
    private string activatorTag = "Player";
    
    // === Pola Prywatne ===
    private PuzzleManager puzzleManager;
    private bool isPressed = false;
    
    // NOWOŚĆ: Licznik colliderów przebywających na płytce
    private int collidersOnPlate = 0; 

    // Metoda wywoływana przez Spawner do inicjalizacji
    public void Initialize(PuzzleManager manager)
    {
        this.puzzleManager = manager;
        this.collidersOnPlate = 0; // Reset licznika przy inicjalizacji
        UpdateVisual(false);
    }
    
    // Metoda pomocnicza do pobierania stabilnej pozycji
    private Vector2Int GetGridPositionFromWorld()
    {
        Vector3 worldPos = transform.position;
        int x = Mathf.RoundToInt(worldPos.x - 0.5f); // Dostosuj offset jeśli konieczne (np. bez -0.5f)
        int y = Mathf.RoundToInt(worldPos.y - 0.5f);
        return new Vector2Int(x, y);
    }
    
    private void UpdateVisual(bool pressed)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = pressed ? pressedSprite : unpressedSprite;
        }
        isPressed = pressed;
    }

    // === GŁÓWNA LOGIKA ZABEZPIECZAJĄCA ===
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Reagujemy tylko na gracza
        if (other.CompareTag(activatorTag))
        {
            // Zwiększamy licznik colliderów
            collidersOnPlate++;

            // Płytkę aktywujemy TYLKO wtedy, gdy jest to PIERWSZY collider (zmiana z 0 na 1)
            // ORAZ płytka nie jest jeszcze logicznie wciśnięta.
            if (collidersOnPlate == 1 && !isPressed)
            {
                ActivatePlateLogic();
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(activatorTag))
        {
            // Zmniejszamy licznik
            collidersOnPlate--;

            // Zabezpieczenie przed błędami fizyki (licznik nie może być ujemny)
            if (collidersOnPlate < 0) collidersOnPlate = 0;

            // Płytkę "odciskamy" TYLKO wtedy, gdy WSZYSTKIE collidery wyszły (licznik spadł do 0)
            if (collidersOnPlate == 0)
            {
                UpdateVisual(false);
            }
        }
    }

    private void ActivatePlateLogic()
    {
        UpdateVisual(true);
        
        // Lazy Loading Managera (na wypadek utraty referencji)
        if (puzzleManager == null)
        {
            puzzleManager = FindFirstObjectByType<PuzzleManager>();
            if (puzzleManager == null) 
            {
                Debug.LogError("BŁĄD: Brak PuzzleManagera!");
                return;
            }
        }
        
        Vector2Int currentPlatePosition = GetGridPositionFromWorld();
        
        // Wywołanie logiki zagadki (tylko raz!)
        puzzleManager.CheckPressurePlate(currentPlatePosition);
        
        Debug.Log($"Płytka aktywowana (Single Trigger). Pozycja: {currentPlatePosition}");
    }
}