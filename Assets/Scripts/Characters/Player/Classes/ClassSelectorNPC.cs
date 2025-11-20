// Plik: ClassSelectorNPC.cs
using UnityEngine;

public class ClassSelectorNPC : MonoBehaviour
{
    public ClassData classToAssign;
    private bool playerCanInteract = false;

    void Update()
    {
        if (playerCanInteract && Input.GetKeyDown(KeyCode.E))
        {
            playerCanInteract = false; 
            ClassSwapManager.Instance.SwapToClass(classToAssign);
        }
    }

    // Ta metoda jest wywoływana, gdy JAKIKOLWIEK collider na tym obiekcie
    // lub jego dzieciach (ustawiony jako trigger) zostanie aktywowany.
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Zabezpieczenie przed błędami
        if (other == null || other.gameObject == null) return;

        // Logika jest teraz prosta, bo skrypt jest na głównym obiekcie:
        // 1. Czy obiekt, który wszedł, jest graczem?
        // 2. Czy JA jestem NPC?
        if (other.CompareTag("Player") && this.gameObject.CompareTag("NPC"))
        {
            playerCanInteract = true;
            Debug.Log("Naciśnij E, aby przejąć kontrolę nad " + classToAssign.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || other.gameObject == null) return;
        
        if (other.CompareTag("Player"))
        {
            playerCanInteract = false;
        }
    }
}