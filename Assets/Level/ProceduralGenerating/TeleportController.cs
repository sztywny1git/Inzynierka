using UnityEngine;

public class TeleportController : MonoBehaviour
{
    public enum TeleportType { ToBossRoom, FromBossRoom }
    public TeleportType type = TeleportType.ToBossRoom;

    public Vector2 targetPosition; 

    public string targetSceneName = ""; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PerformTeleport(other.gameObject);
        }
    }

    private void PerformTeleport(GameObject player)
    {
        //Debug.Log($"Teleporting Player from {gameObject.name}...");

        if (!string.IsNullOrEmpty(targetSceneName) && targetSceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
        {

        }
        else
        {

            player.transform.position = new Vector3(targetPosition.x, targetPosition.y, player.transform.position.z);
        }

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