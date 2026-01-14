using UnityEngine;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private ProfileSelectionPanel profileSelectionPanel;

    private void Awake()
    {
        playButton.onClick.AddListener(OnPlayButtonPressed);
    }

    private void OnPlayButtonPressed()
    {
        profileSelectionPanel.Show();
        this.gameObject.SetActive(false);
    }
}