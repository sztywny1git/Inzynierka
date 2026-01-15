using UnityEngine;

public class TeleportController : MonoBehaviour
{
    // Określa, czy ten teleport jest wejściem do Boss Roomu, czy wyjściem
    public enum TeleportType { ToBossRoom, FromBossRoom }
    public TeleportType type = TeleportType.ToBossRoom;

    // Pozycja docelowa: cel teleportacji w innym miejscu/scenie
    public Vector2 targetPosition; 

    // Nazwa sceny docelowej (jeśli teleportujemy się między scenami)
    // Opcjonalnie: Używane, jeśli Boss Room jest na innej scenie niż Reszta Lochu
    public string targetSceneName = ""; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sprawdź, czy obiekt, który wszedł w trigger, to Gracz
        if (other.CompareTag("Player"))
        {
            PerformTeleport(other.gameObject);
        }
    }

    private void PerformTeleport(GameObject player)
    {
        Debug.Log($"Teleporting Player from {gameObject.name}...");

        if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {
            // Ładowanie innej sceny
            // UWAGA: Ta implementacja jest uproszczona. W pełnej grze musisz użyć 
            // menedżera sceny, aby poprawnie załadować scenę i umieścić gracza.
            
            // Przykład użycia:
            // SceneManager.LoadScene(targetSceneName);
            // GameStateManager.SetNextPlayerPosition(targetPosition); 
        }
        else
        {
            // Teleportacja w ramach tej samej sceny
            // Pozycję ustawiamy w 3D, ale Z=0 dla 2D
            player.transform.position = new Vector3(targetPosition.x, targetPosition.y, player.transform.position.z);
        }
        
        // Dezaktywacja kolizji na krótki czas, by zapobiec natychmiastowej ponownej teleportacji
        // player.GetComponent<Collider2D>().enabled = false;
        // Invoke("ReEnablePlayerCollider", 0.5f);
    }

    // Opcjonalne: Użyj metody publicznej do włączania/wyłączania teleportu
    public void ActivateTeleport()
    {
        gameObject.SetActive(true);
    }
    
    public void DeactivateTeleport()
    {
        gameObject.SetActive(false);
    }
}