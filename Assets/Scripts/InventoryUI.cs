using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Panel")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private KeyCode toggleKey = KeyCode.E;

    private void Awake()
    {
        if (inventoryPanel != null)
        {
            // Domyślnie ukryj ekwipunek na starcie
            if (inventoryPanel.activeSelf)
                inventoryPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("InventoryUI: Brak przypisanego panelu ekwipunku");
            return;
        }
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }
}
