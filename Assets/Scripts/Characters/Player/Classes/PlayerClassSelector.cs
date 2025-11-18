using UnityEngine;

public class PlayerClassSelector : MonoBehaviour
{
    public ClassData classToAssign;
    private bool playerInside = false;
    private PlayerClassController player;

    void Update()
    {
        // Change class when player presses E inside the trigger
        if (playerInside && Input.GetKeyDown(KeyCode.E))
        {
            player.ApplyClass(classToAssign);
            Debug.Log("Class changed to: " + classToAssign.name);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect player entering trigger
        if (other.TryGetComponent(out PlayerClassController p))
        {
            playerInside = true;
            player = p;
            Debug.Log("Press E to change class to " + classToAssign.name);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Detect player leaving trigger
        if (other.TryGetComponent(out PlayerClassController p))
        {
            playerInside = false;
            player = null;
        }
    }
}
