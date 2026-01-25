using UnityEngine;

public class GateComponent : MonoBehaviour
{
    [Header("Komponenty")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BoxCollider2D physicsCollider;

    [Header("Sprite'y")]
    [SerializeField] private Sprite horizontalClosed;
    [SerializeField] private Sprite horizontalOpen;
    [SerializeField] private Sprite verticalClosed;
    [SerializeField] private Sprite verticalOpen;
    [SerializeField, HideInInspector] private bool isHorizontal = true;
    [SerializeField, HideInInspector] private bool isOpen = false;

    public void Initialize(bool horizontal, bool startOpened)
    {
        this.isHorizontal = horizontal;
        this.isOpen = startOpened;
        
        //Debug.Log($"Gate Init: Horizontal={horizontal}, Open={startOpened}");
        
        UpdateVisuals();
    }

    public void Open()
    {
        //Debug.Log($"GateComponent: Próba otwarcia. Obecny stan isOpen: {isOpen}");
        
        if (isOpen) return;

        isOpen = true;
        //Debug.Log("GateComponent: Zmieniam flagę isOpen na TRUE. Aktualizuję wygląd...");
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("FATAL ERROR: GateComponent nie ma przypisanego SpriteRenderer w Inspektorze!");
            return;
        }

        Sprite targetSprite = null;

        if (isHorizontal)
        {
            targetSprite = isOpen ? horizontalOpen : horizontalClosed;
            //Debug.Log($"Wybieram sprite POZIOMY. Otwarty? {isOpen}");
        }
        else
        {
            targetSprite = isOpen ? verticalOpen : verticalClosed;
            //Debug.Log($"Wybieram sprite PIONOWY. Otwarty? {isOpen}");
        }

        if (targetSprite == null)
        {
            Debug.LogError("ERROR: Wybrany Sprite jest NULL! Sprawdź czy przypisałeś obrazki w Inspektorze prefabu Bramy.");
        }
        else
        {
            spriteRenderer.sprite = targetSprite;
            //Debug.Log($"Sukces: Zmieniono sprite na {targetSprite.name}");
        }

        if (physicsCollider != null)
        {
            physicsCollider.enabled = !isOpen;
            //Debug.Log($"Collider {(isOpen ? "WYŁĄCZONY" : "WŁĄCZONY")}");
        }
    }
}