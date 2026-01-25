using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BinaryPuzzleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject puzzlePanel; 
    [SerializeField] private TextMeshProUGUI targetNumberText; 
    [SerializeField] private Image statusLamp; 
    [SerializeField] private Button closeButton; 

    [Header("Switches")]
    [Tooltip("Lista Twoich sliderów ToggleSwitch. Element 0 = bit 1, Element 7 = bit 128")]
    [SerializeField] private List<ToggleSwitch> bitSwitches; 

    [Header("Colors")]
    [SerializeField] private Color unsolvedColor = Color.red;
    [SerializeField] private Color solvedColor = Color.green;

    private BinaryConsoleComponent activeConsole; 
    private int targetValue;
    private bool isSolved = false;

    private void Start()
    {
        if(puzzlePanel != null) puzzlePanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
    }

    public void OpenPuzzle(int target, BinaryConsoleComponent console)
    {
        //Debug.Log($"[UI] Otwieranie panelu. Cel: {target}");
        
        activeConsole = console;
        targetValue = target;
        isSolved = false;

        if (puzzlePanel != null) puzzlePanel.SetActive(true);

        if(targetNumberText != null) targetNumberText.text = target.ToString();
        if(statusLamp != null) statusLamp.color = unsolvedColor;

        foreach (var ts in bitSwitches)
        {
            if (ts != null) ts.ToggleByGroupManager(false);
        }

        PauseGame();
    }

    public void ClosePanel()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        activeConsole = null;

        ResumeGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; 
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    public void CheckSolution()
    {
        if (isSolved) return;

        int currentValue = 0;

        for (int i = 0; i < bitSwitches.Count; i++)
        {
            if (bitSwitches[i] != null && bitSwitches[i].CurrentValue)
            {
                currentValue += (1 << i);
            }
        }

        if (currentValue == targetValue)
        {
            OnSuccess();
        }
    }

    private void OnSuccess()
    {
        isSolved = true;
        if(statusLamp != null) statusLamp.color = solvedColor;
        
        if (activeConsole != null)
        {
            activeConsole.NotifySolved();
        }
        
        StartCoroutine(WaitAndClose());
    }

    private System.Collections.IEnumerator WaitAndClose()
    {
        yield return new WaitForSecondsRealtime(1.0f);
        ClosePanel();
    }
}