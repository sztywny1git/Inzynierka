using UnityEngine;

public class PressurePlateComponent : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private Sprite unpressedSprite; 
    [SerializeField]
    private Sprite pressedSprite; 
    
    [Header("Logic")]
    [Tooltip("Tag gracza lub obiektu, który ma aktywować płytkę.")]
    [SerializeField]
    private string activatorTag = "Player";
    
    private PuzzleManager puzzleManager;
    private bool isPressed = false;
    
    private int collidersOnPlate = 0; 

    public void Initialize(PuzzleManager manager)
    {
        this.puzzleManager = manager;
        this.collidersOnPlate = 0; 
        UpdateVisual(false);
    }
    
    private Vector2Int GetGridPositionFromWorld()
    {
        Vector3 worldPos = transform.position;
        int x = Mathf.RoundToInt(worldPos.x - 0.5f); 
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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(activatorTag))
        {
            collidersOnPlate++;

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
            collidersOnPlate--;

            if (collidersOnPlate < 0) collidersOnPlate = 0;

            if (collidersOnPlate == 0)
            {
                UpdateVisual(false);
            }
        }
    }

    private void ActivatePlateLogic()
    {
        UpdateVisual(true);
        
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
        
        puzzleManager.CheckPressurePlate(currentPlatePosition);
        
        Debug.Log($"Płytka aktywowana (Single Trigger). Pozycja: {currentPlatePosition}");
    }
}