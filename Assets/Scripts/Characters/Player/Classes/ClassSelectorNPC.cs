// Plik: ClassSelectorNPC.cs
using UnityEngine;

public class ClassSelectorNPC : MonoBehaviour
{
    public ClassData classToAssign;
    private bool playerCanInteract = false;

    void Update()
    {
        // Sprawdzamy, czy można wejść w interakcję i czy naciśnięto klawisz
        if (playerCanInteract && Input.GetKeyDown(KeyCode.E))
        {
            // Resetujemy flagę, aby uniknąć wielokrotnego wywołania
            playerCanInteract = false; 
            ClassSwapManager.Instance.SwapToClass(classToAssign);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // KROK 1: ZABEZPIECZENIE PRZED BŁĘDEM
        // Sprawdź, czy obiekt, który wszedł, w ogóle istnieje
        if (other == null || other.gameObject == null)
        {
            return;
        }

        // KROK 2: POPRAWIONA LOGIKA INTERAKCJI
        // Interakcja jest możliwa tylko wtedy, gdy:
        // - Obiekt, który wszedł, jest graczem ("Player")
        // - Ten obiekt (NPC) nie jest graczem ("NPC")
        if (other.CompareTag("Player") && this.gameObject.CompareTag("NPC"))
        {
            playerCanInteract = true;
            Debug.Log("Naciśnij E, aby przejąć kontrolę nad " + classToAssign.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // ZABEZPIECZENIE PRZED BŁĘDEM
        if (other == null || other.gameObject == null)
        {
            return;
        }

        // Zawsze resetuj interakcję, gdy gracz wychodzi z zasięgu
        if (other.CompareTag("Player"))
        {
            playerCanInteract = false;
        }
    }
}