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
        Debug.Log($"[UI] Otwieranie panelu. Cel: {target}");
        
        activeConsole = console;
        targetValue = target;
        isSolved = false;

        // 1. Włącz panel
        if (puzzlePanel != null) puzzlePanel.SetActive(true);

        // 2. Ustaw wartości
        if(targetNumberText != null) targetNumberText.text = target.ToString();
        if(statusLamp != null) statusLamp.color = unsolvedColor;

        // 3. Zresetuj switche (teraz zadziała, bo panel jest aktywny)
        foreach (var ts in bitSwitches)
        {
            if (ts != null) ts.ToggleByGroupManager(false);
        }

        // 4. ZATRZYMAJ GRĘ I POKAŻ MYSZKĘ
        PauseGame();
    }

    public void ClosePanel()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        activeConsole = null;

        // 5. WZNOW GRĘ
        ResumeGame();
    }

    private void PauseGame()
    {
        // Zatrzymuje czas (ruch gracza, fizykę)
        Time.timeScale = 0f; 
        
        // Odblokowuje kursor i czyni go widocznym
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        // Wznawia czas
        Time.timeScale = 1f;

        // Opcjonalnie: Jeśli Twoja gra normalnie ukrywa kursor (np. FPS/TopDown), zablokuj go z powrotem.
        // Jeśli gra zawsze ma widoczny kursor, możesz usunąć te linie.
        // Cursor.lockState = CursorLockMode.Locked; 
        // Cursor.visible = false; 
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

        // Debug.Log($"Suma: {currentValue} / Cel: {targetValue}");

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

        // Zamknij panel po 1 sekundzie (używamy coroutine lub ignorujemy timeScale przy Invoke, 
        // ale ponieważ czas stoi (timeScale=0), Invoke może nie zadziałać poprawnie!)
        
        // Zamiast Invoke, użyjmy triku z coroutine ignorującą TimeScale, 
        // albo po prostu zamknijmy ręcznie przez przycisk gracza.
        // Dla uproszczenia tutaj: wymuś zamknięcie od razu lub użyj real-time coroutine.
        
        StartCoroutine(WaitAndClose());
    }

    private System.Collections.IEnumerator WaitAndClose()
    {
        // Czekaj 1 sekundę czasu rzeczywistego (nawet jak gra jest zapauzowana)
        yield return new WaitForSecondsRealtime(1.0f);
        ClosePanel();
    }
}