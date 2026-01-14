using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI slotInfoText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;

    public void Refresh(ProfileSummary summary)
    {
        if (summary.IsOccupied)
        {
            string runStatus = summary.HasActiveRun ? $" (Run: Lvl {summary.CurrentLevel})" : " (In Hub)";
            slotInfoText.text = $"{summary.CharacterClassName}{runStatus}";
            selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "Load";
            deleteButton.gameObject.SetActive(true);
        }
        else
        {
            slotInfoText.text = "Empty Slot";
            selectButton.GetComponentInChildren<TextMeshProUGUI>().text = "New Game";
            deleteButton.gameObject.SetActive(false);
        }
    }
    
    public Button GetSelectButton() => selectButton;
    public Button GetDeleteButton() => deleteButton;
}