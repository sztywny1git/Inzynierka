// Plik: ClassSwapManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine; // Poprawna przestrzeń nazw dla nowszych wersji Cinemachine

public class ClassSwapManager : MonoBehaviour
{
    public static ClassSwapManager Instance { get; private set; }

    [Header("Setup")]
    [SerializeField] private ClassData startingClass;
    [SerializeField] private CinemachineCamera Camera; // Poprawny typ komponentu kamery

    private List<ControllableCharacter> allCharacters = new List<ControllableCharacter>();
    private ControllableCharacter activeCharacter;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        // --- POPRAWIONA LINIA PONIŻEJ ---
        // Używamy nowej, szybszej metody FindObjectsByType z opcją sortowania None.
        allCharacters = FindObjectsByType<ControllableCharacter>(FindObjectsSortMode.None).ToList();

        foreach (var character in allCharacters)
        {
            character.DisablePlayerControl();
        }

        var startingCharacter = allCharacters.FirstOrDefault(c => c.characterClassData == startingClass);
        if (startingCharacter != null)
        {
            SwapToCharacter(startingCharacter);
        }
        else
        {
            Debug.LogError("Nie znaleziono na scenie postaci startowej!");
        }
    }

    public void SwapToClass(ClassData newClassData)
    {
        var targetCharacter = allCharacters.FirstOrDefault(c => c.characterClassData == newClassData);
        if (targetCharacter != null && targetCharacter != activeCharacter)
        {
            SwapToCharacter(targetCharacter);
        }
        else if (targetCharacter == activeCharacter)
        {
            Debug.Log("Już kontrolujesz tę postać!");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono na scenie postaci dla klasy: " + newClassData.name);
        }
    }

    private void SwapToCharacter(ControllableCharacter newCharacter)
    {
        if (activeCharacter != null)
        {
            activeCharacter.DisablePlayerControl();
        }

        activeCharacter = newCharacter;
        activeCharacter.EnablePlayerControl();

        if (Camera != null)
        {
            Camera.Follow = activeCharacter.transform;
        }
    }
}