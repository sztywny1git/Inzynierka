using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PuzzleManager : MonoBehaviour
{
    [Header("Dostępne Szablony Zagadek")]
    [Tooltip("Lista szablonów, które mogą być użyte w lochu. Np. Sequence, MovableBlocks, etc.")]
    [SerializeField]
    private List<PuzzleTemplate> availablePuzzles;

    // Słownik przechowujący dane zagadek dla każdego pokoju
    private Dictionary<Vector2Int, PuzzleData> activePuzzles = new Dictionary<Vector2Int, PuzzleData>();

    // Referencja ustawiana przez DungeonGenerator
    [SerializeField] private RoomFirstDungeonGenerator dungeonGenerator;

    [System.Serializable] // To pozwala Unity zapisać tę strukturę
    public struct PuzzleSaveEntry
    {
        public Vector2Int RoomCenter;
        public PuzzleData Data;
    }

    [SerializeField, HideInInspector] 
    private List<PuzzleSaveEntry> savedPuzzles = new List<PuzzleSaveEntry>();
    public HashSet<Vector2Int> ReservedPuzzlePositions = new HashSet<Vector2Int>(); 

    [Header("Puzzle Feedback")]
    [SerializeField] private AudioClip correctStepSFX;
    [SerializeField] private AudioClip incorrectStepSFX;
    [SerializeField] private GameObject successVFXPrefab; // Opcjonalny efekt wizualny na płytce

    private AudioSource audioSource;

    private void Awake()
    {
        // Jeśli słownik jest pusty, ale mamy zapisane dane na liście...
        if (activePuzzles.Count == 0 && savedPuzzles.Count > 0)
        {
            Debug.Log("PuzzleManager: Odtwarzanie zagadek z pamięci (List -> Dictionary)...");
            
            foreach (var entry in savedPuzzles)
            {
                if (!activePuzzles.ContainsKey(entry.RoomCenter))
                {
                    activePuzzles.Add(entry.RoomCenter, entry.Data);
                }
            }
            Debug.Log($"PuzzleManager: Odtworzono {activePuzzles.Count} zagadek.");
        }
        
        audioSource = GetComponent<AudioSource>(); // Upewnij się, że PuzzleManager ma AudioSource
        if (audioSource == null)
        {
            Debug.LogWarning("Brak komponentu AudioSource na PuzzleManager. Dźwięki nie będą odtwarzane.");
        }
    }

    public RoomFirstDungeonGenerator DungeonGenerator 
    {
        get
        {
            // Jeśli referencja jest null (utracona), spróbuj ją znaleźć na scenie
            if (dungeonGenerator == null)
            {
                dungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
                if (dungeonGenerator == null)
                {
                    Debug.LogError("FATAL ERROR: Nie znaleziono RoomFirstDungeonGenerator na scenie!");
                }
            }
            return dungeonGenerator;
        }
        set
        {
            dungeonGenerator = value;
        }
    }

    // === METODA INICJALIZUJĄCA DANE ZAGADEK ===
    public void PreparePuzzles(List<(BoundsInt room, bool isKey)> puzzleRoomsWithContext)
    {
        activePuzzles.Clear();
        savedPuzzles.Clear();
        
        if (availablePuzzles == null || availablePuzzles.Count == 0)
        {
            Debug.LogError("Brak dostępnych szablonów zagadek. Przypisz je w Inspektorze!");
            return;
        }

        Debug.Log($"Liczba dostepnych pokoi z zagadkami {availablePuzzles.Count}");

        List<PuzzleTemplate> templatesToUse = new List<PuzzleTemplate>();
        int numberOfRooms = puzzleRoomsWithContext.Count;

        for (int i = 0; i < numberOfRooms; i++)
        {
            templatesToUse.Add(availablePuzzles[i % availablePuzzles.Count]);
        }

        var shuffledPuzzles = availablePuzzles.OrderBy(x => Random.value).ToList();
        

        // 2. Generowanie danych dla każdego pokoju
        for (int i = 0; i < numberOfRooms; i++)
        {
            BoundsInt room = puzzleRoomsWithContext[i].room;
            bool isKey = puzzleRoomsWithContext[i].isKey;           
            Vector2Int roomCenter = GetRoomCenter(room);

            PuzzleTemplate template = shuffledPuzzles[i % shuffledPuzzles.Count];
            PuzzleData data = template.GeneratePuzzleData(room);

            data.PuzzleObjectPosition = roomCenter;
            data.IsKeyPuzzle = puzzleRoomsWithContext[i].isKey;

            // === ZMIANA: Dodaj używając środka jako klucza ===
            if (!activePuzzles.ContainsKey(roomCenter))
            {
                activePuzzles.Add(roomCenter, data);
                savedPuzzles.Add(new PuzzleSaveEntry 
                { 
                    RoomCenter = roomCenter, 
                    Data = data 
                });
            }
        
            // =========================================================
            // === FILTRACJA I USTAWIANIE OSTATECZNYCH POZYCJI (KLUCZOWE) ===
            // =========================================================
            
            if (DungeonGenerator != null)
            {
                // 1. Filtracja płytek ciśnieniowych (jeśli istnieją)
                if (data.PressurePlatePositions != null && data.PressurePlatePositions.Count > 0)
                {
                    data.PressurePlatePositions = FilterToFloorOnly(data.PressurePlatePositions);
                    data.PressurePlatePositions = data.PressurePlatePositions.Distinct().ToList();

                    int validPlateCount = data.PressurePlatePositions.Count;

                    if (validPlateCount > 0)
                    {
                        // Ustalamy długość sekwencji (np. 5 kroków, lub tyle ile było w szablonie)
                        int sequenceLength = 5; 
                        
                        // Czyścimy starą sekwencję, która mogła odwoływać się do nieistniejących płytek
                        data.CorrectSequence.Clear();

                        // Generujemy nową, bezpieczną sekwencję
                        for (int k = 0; k < sequenceLength; k++)
                        {
                            // Kluczowe: Losujemy indeks tylko z zakresu [0, validPlateCount - 1]
                            int safeIndex = Random.Range(0, validPlateCount);
                            data.CorrectSequence.Add(safeIndex);
                        }
                        
                        // Debug dla pewności
                        // Debug.Log($"Zregenerowano sekwencję dla pokoju {roomCenter}. Płytek: {validPlateCount}. Sekwencja: {string.Join(",", data.CorrectSequence)}");
                    }
                    else
                    {
                        // Jeśli filtracja usunęła WSZYSTKIE płytki (np. pokój jest za mały)
                        Debug.LogError($"BŁĄD KRYTYCZNY: Pokój {room} po filtracji nie ma miejsca na płytki! Zagadka będzie pusta.");
                        // W takim wypadku najlepiej nie dodawać tej zagadki w ogóle:
                        continue; 
                    }

                    if (data.PressurePlatePositions.Contains(data.PuzzleObjectPosition))
                    {
                        // Znajdź najbliższe wolne miejsce obok, które NIE JEST płytką
                        Vector2Int safePos = FindSafePositionForReward(
                            data.PuzzleObjectPosition, 
                            data.PressurePlatePositions, 
                            DungeonGenerator.floorPositions, 
                            room
                        );

                        if (safePos != Vector2Int.zero)
                        {
                            data.PuzzleObjectPosition = safePos;
                            Debug.Log($"Skrzynia w pokoju {roomCenter} przesunięta na {safePos}, aby odsłonić płytkę.");
                        }
                        else
                        {
                            // Sytuacja krytyczna (brak miejsca): Usuwamy płytkę, która jest pod skrzynią
                            Debug.LogWarning("Brak miejsca dla skrzyni! Usuwanie kolidującej płytki.");
                            data.PressurePlatePositions.Remove(data.PuzzleObjectPosition);
                            
                            // Tutaj warto byłoby ponownie zregenerować sekwencję, jeśli usunięta płytka była wymagana,
                            // ale z logicznego punktu widzenia, jeśli usunęliśmy element listy, to indeksy się przesuną.
                            // Dla bezpieczeństwa w takim rzadkim przypadku po prostu usuń płytkę.
                        }
                    }
                }
                
                // 2. Obsługa pozycji Bramy/Konsoli (GatePosition)
                // Używamy tego samego fallbacku dla wszystkich obiektów, które mają znaleźć się na podłodze.
                if (!DungeonGenerator.floorPositions.Contains(data.PuzzleObjectPosition))
                {
                    // Pozycja wylądowała na ścianie/granicy BoundsInt.
                    Vector2Int fallbackPos = FindNearestFloorTile(data.PuzzleObjectPosition, DungeonGenerator.floorPositions, room);
                    
                    if (fallbackPos != Vector2Int.zero)
                    {
                        data.PuzzleObjectPosition = fallbackPos;
                        Debug.LogWarning($"Gate/Console na pozycji ({data.PuzzleObjectPosition}) wymagało korekty.");
                    }
                    else
                    {
                        // Ten błąd oznacza, że pokój jest całkowicie otoczony ścianami lub jest zbyt mały
                        Debug.LogError($"Nie znaleziono bezpiecznej pozycji podłogi dla Gate/Console w pokoju {room}.");
                    }
                }
                
                // 3. Obsługa kluczowej zagadki (Bramy)
                if (isKey)
                {
                    Vector2Int exitPoint = DungeonGenerator.GetCorridorExitPoint(room, DungeonGenerator.MainPathRooms); 
                    
                    if (exitPoint != Vector2Int.zero)
                    {
                        data.GatePosition = exitPoint;
                    }
                    else
                    {
                        Debug.LogError($"PreparePuzzles: Nie udało się wyznaczyć GatePosition dla pokoju {roomCenter}!");
                        // Fallback: Jeśli nie znaleziono wyjścia, gra nie może kontynuować.
                    }
                }
            }
            
            data.IsKeyPuzzle = isKey;       
            if (!activePuzzles.ContainsKey(roomCenter))
            {
                activePuzzles.Add(roomCenter, data);
            }
        }
        Debug.Log($"PreparePuzzles: Zapisano {savedPuzzles.Count} zagadek do pamięci trwałej.");
    }
    
    // ====================================================================
    // === NOWA METODA: Spawnowanie Fizycznych Obiektów Zagadki ===
    // ====================================================================

    public void SpawnPuzzleObjects(BoundsInt roomLimits, Spawner spawner)
    {
        if (spawner == null)
        {
            Debug.LogError("Spawner nie jest przypisany w DungeonGenerator lub przekazany jako null.");
            return;
        }
        
        foreach (var entry in activePuzzles)
        {
            Vector2Int roomCenter = entry.Key; // Klucz to teraz Vector2Int
            PuzzleData puzzleValue = entry.Value;

            // === ZMIANA: Odzyskaj BoundsInt pokoju na podstawie środka ===
            // Musisz mieć dostęp do DungeonGeneratora. Zakładam, że masz property 'DungeonGenerator'
            if (DungeonGenerator == null) 
            {
                DungeonGenerator = FindFirstObjectByType<RoomFirstDungeonGenerator>();
            }

            // Znajdź pokój, który zawiera ten środek
            BoundsInt room = DungeonGenerator.FindRoomContainingPoint(DungeonGenerator.allRooms, roomCenter);
            if (activePuzzles.TryGetValue(roomCenter, out PuzzleData puzzle))
            {
                // 1. Spawnowanie GŁÓWNEGO OBIEKTU ZAGADKI (Konsoli/Płytek/Skrzyni)
                if (puzzle.PuzzleTemplateType == nameof(BinarySwitchPuzzleTemplate)) 
                {
                    // Spawnowanie konsoli (używa skorygowanej pozycji w POKOJU)
                    spawner.SpawnBinaryConsole(puzzle.PuzzleObjectPosition, puzzle);
                }
                else // Obsługa Pressure Plate / Skrzyni
                {
                    // Spawnowanie płytek ciśnieniowych (jeśli istnieją)
                    foreach (var pos in puzzle.PressurePlatePositions)
                    {
                        spawner.SpawnPressurePlate(pos, DungeonGenerator); 
                    }
                }
                
                // 2. === KOREKTA BŁĘDU: Spawnowanie BRAMY (dla WSZYSTKICH kluczowych zagadek) ===
                if (puzzle.IsKeyPuzzle)
                {
                    // === FIX: POPRAWNE SPRAWDZANIE STANU DLA RÓŻNYCH TYPÓW ZAGADEK ===
                    bool isSolved = false;

                    if (puzzle.PuzzleTemplateType == nameof(BinarySwitchPuzzleTemplate))
                    {
                        // Dla zagadek binarnych "rozwiązana" oznacza CurrentProgressIndex == int.MaxValue
                        // (Tak ustaliliśmy w metodzie SolveBinaryPuzzle)
                        isSolved = (puzzle.CurrentProgressIndex == int.MaxValue);
                    }
                    else
                    {
                        // Dla płytek i innych sekwencyjnych: postęp >= długość sekwencji
                        // (Zabezpieczenie: jeśli lista jest pusta, to NIE jest rozwiązana, chyba że to zamierzone)
                        if (puzzle.CorrectSequence != null && puzzle.CorrectSequence.Count > 0)
                        {
                            isSolved = (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count);
                        }
                        else
                        {
                            // Jeśli sekwencja jest pusta (a nie jest to binary), to zagadka niegotowa -> uznajemy za nierozwiązaną
                            isSolved = false;
                        }
                    }

                    if (puzzle.GatePosition != Vector2Int.zero)
                    {
                        // Przekazujemy poprawny stan 'isSolved'
                        spawner.SpawnGate(puzzle.GatePosition, DungeonGenerator, isSolved); 
                    }
                    else
                    {
                        Debug.LogError($"Błąd: Kluczowa zagadka w pokoju {roomCenter} nie ma ustawionego GatePosition!");
                    }
                }
            }
        }
    }

    // === METODA OBSŁUGUJĄCA INTERAKCJĘ GRACZA ===

    public bool CheckPressurePlate(Vector2Int position)
    {
        Debug.Log("metoda CheckPressurePlate wywolana");

        RoomFirstDungeonGenerator generator = DungeonGenerator; // Użyj właściwości

        if (generator == null || generator.allRooms == null) // Sprawdź, czy odzyskanie się powiodło
        { 
            Debug.LogError("DungeonGenerator nie został odzyskany. Przerwanie CheckPressurePlate.");
            return false;
        }
        
        // Znajdź pokój zawierający naciśniętą pozycję, używając metody z Generatora
        BoundsInt currentRoomBounds = generator.FindRoomContainingPoint(DungeonGenerator.allRooms, position);

        Vector2Int roomCenterKey = GetRoomCenter(currentRoomBounds);

        Debug.Log($"Płytka aktywowana na pozycji: {position}. Znaleziony pokój: {roomCenterKey}");

        Debug.Log($"Próba użycia pokoju: {roomCenterKey}. Liczba aktywnych zagadek: {activePuzzles.Count}");


        
        foreach (var key in activePuzzles.Keys)
        {
            Debug.Log($"[KEY] {key}");
        }


        if (!activePuzzles.ContainsKey(roomCenterKey))
        {
            // === DODATKOWA DIAGNOSTYKA ===
            Debug.LogWarning($"CheckPressurePlate: Brak zagadki dla klucza {roomCenterKey} (Pokój: {currentRoomBounds})");
            
            Debug.Log("--- DOSTĘPNE KLUCZE W SŁOWNIKU ---");
            foreach (var key in activePuzzles.Keys)
            {
                Debug.Log($"Dostępny Klucz: {key} | Czy równy szukanemu? {key == roomCenterKey}");
            }
            Debug.Log("----------------------------------");
            
            return false;
        }

        PuzzleData puzzle = activePuzzles[roomCenterKey];
        
        Debug.Log("--- DOSTĘPNE KLUCZE W SŁOWNIKU ---");
        foreach (var key in activePuzzles.Keys)
        {
              Debug.Log($"Dostępny Klucz: {key} | Czy równy szukanemu? {key == roomCenterKey}");
        }
        Debug.Log("----------------------------------");

        // Logika Sekwencji (przeniesiona z Generatora)
        int pressedPlateIndex = puzzle.PressurePlatePositions.IndexOf(position);

        Debug.Log($"--- Lista płytek dla pokoju o kluczu: {roomCenterKey}--- ");
        foreach (var pos in puzzle.PressurePlatePositions)
        {
              Debug.Log($"Pozycja {pos}");
        }
        Debug.Log("----------------------------------");
        
        if (pressedPlateIndex == -1)
        {
            Debug.LogError($"Błąd! Pozycja {position} nie została znaleziona na liście płytek w tym pokoju! (Pozycje: {string.Join(", ", puzzle.PressurePlatePositions)})");
            return false; // Powinien być zawsze znaleziony
        }
        
        // Sprawdzenie, czy zagadka już jest rozwiązana
        if (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count)
        {
            Debug.Log("Zagadka już rozwiązana.");
            return true;
        }

        int requiredPlateIndex = puzzle.CorrectSequence[puzzle.CurrentProgressIndex];

        Debug.Log($"Wciśnięto płytkę o indeksie: {pressedPlateIndex}. Wymagany indeks: {requiredPlateIndex}");

        if (pressedPlateIndex == requiredPlateIndex)
        {
            // Poprawny krok
            puzzle.CurrentProgressIndex++;
            Debug.Log($"Poprawna płytka naciśnięta. Postęp: {puzzle.CurrentProgressIndex}/{puzzle.CorrectSequence.Count}");

            // 1. Odtwórz dźwięk sukcesu
            if (audioSource != null && correctStepSFX != null)
            {
                audioSource.PlayOneShot(correctStepSFX);
            }

            // 2. Aktywuj tymczasowy efekt wizualny na płytce
            if (DungeonGenerator.spawner != null && successVFXPrefab != null)
            {
                // Możesz użyć Spawnera do tymczasowego spawnowania VFX nad płytką
                // (Zakładając, że Spawner ma metodę do spawnowania efektów)
                // DungeonGenerator.spawner.SpawnVFX(successVFXPrefab, position);
            }

            if (puzzle.CurrentProgressIndex >= puzzle.CorrectSequence.Count)
            {
                // SEKWEKCJA UKOŃCZONA!
                if (puzzle.IsKeyPuzzle)
                {
                    // KLUCZOWA ZAGADKA: Usuń bramę blokującą KORYTARZ.
                    if (DungeonGenerator.spawner != null)
                    {
                        // === ZMIANA: Otwórz zamiast usuwać ===
                        DungeonGenerator.spawner.OpenGate(puzzle.GatePosition); 
                    }
                    Debug.Log("Kluczowa zagadka rozwiązana! Brama otwarta.");
                }
                else
                {
                    // OPCJONALNA ZAGADKA: Aktywuj nagrodę w POKOJU (np. skrzynia jest otwieralna).
                    if (DungeonGenerator.spawner != null)
                    {
                        // Używamy PuzzleObjectPosition (to jest środek pokoju ustalony w PreparePuzzles)
                        DungeonGenerator.spawner.SpawnTreasureChest(puzzle.PuzzleObjectPosition, DungeonGenerator);
                        
                        // Odtwórz dźwięk pojawienia się nagrody (jeśli masz inny niż successSFX)
                        // audioSource.PlayOneShot(chestSpawnSFX); 
                    }
                    Debug.Log("Opcjonalna zagadka rozwiązana! Skrzynia zespawnowana.");
                }
                return true;
            }
        }
        else
        {
            // Zły krok - Reset
            puzzle.CurrentProgressIndex = 0;

            // 1. Odtwórz dźwięk błędu/resetu
            if (audioSource != null && incorrectStepSFX != null)
            {
                audioSource.PlayOneShot(incorrectStepSFX);
            }

            Debug.Log("Niepoprawna płytka naciśnięta. Sekwencja zresetowana.");
        }

        activePuzzles[roomCenterKey] = puzzle;
        return false;
    }

    public void SolveBinaryPuzzle(Vector2Int consolePosition)
    {
        // Musimy znaleźć klucz pokoju (środek) na podstawie pozycji konsoli.
        // Ponieważ konsola STOI na środku, consolePosition to prawdopodobnie nasz klucz.
        
        if (activePuzzles.ContainsKey(consolePosition))
        {
            PuzzleData puzzle = activePuzzles[consolePosition];
            
            // Oznacz jako rozwiązaną (ustaw indeks na max)
            puzzle.CurrentProgressIndex = int.MaxValue; 
            
            if (puzzle.IsKeyPuzzle)
            {
                if (DungeonGenerator.spawner != null)
                {
                    // Otwórz bramę przypisaną do tego pokoju
                    DungeonGenerator.spawner.OpenGate(puzzle.GatePosition);
                    Debug.Log($"[PuzzleManager] Otwarto bramę dla zagadki binarnej w pokoju {consolePosition}");
                }
            }
            
            // Zapisz stan
            activePuzzles[consolePosition] = puzzle;
        }
        else
        {
            Debug.LogError($"[PuzzleManager] Nie znaleziono danych zagadki dla konsoli na pozycji {consolePosition}");
        }
    }

    private List<Vector2Int> FilterWallPositions(List<Vector2Int> positions)
    {
        // Używamy WallPositions z generatora
        HashSet<Vector2Int> wallPositions = DungeonGenerator.WallPositions;
        
        // Zwracamy tylko te pozycje, które NIE SĄ ścianą.
        return positions.Where(pos => !wallPositions.Contains(pos)).ToList();
    }

    private List<Vector2Int> FilterToFloorOnly(List<Vector2Int> positions)
    {
        // Używamy floorPositions z generatora jako białej listy
        HashSet<Vector2Int> floorPositions = DungeonGenerator.floorPositions;
        
        // Zwracamy tylko te pozycje, które są DEFINITYWNIE na podłodze.
        return positions
            .Where(pos => floorPositions.Contains(pos))
            .ToList();
    }

    private Vector2Int FindNearestFloorTile(Vector2Int startPos, HashSet<Vector2Int> floorPositions, BoundsInt room)
    {
        // === NOWA, PROSTA I NIEZAWODNA LOGIKA WYSZUKIWANIA ===
        // W pierwszej kolejności sprawdzamy mały promień wokół centrum (jak dotychczas).
        int maxRadius = 5; 
        
        for (int r = 0; r <= maxRadius; r++)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    Vector2Int checkPos = startPos + new Vector2Int(x, y);
                    
                    // 1. Czy jest na liście wszystkich podłóg?
                    if (floorPositions.Contains(checkPos)) 
                    {
                        // 2. I czy jest w granicach naszego pokoju?
                        if (room.Contains(new Vector3Int(checkPos.x, checkPos.y, 0)))
                        {
                            return checkPos; // Znaleziono w promieniu!
                        }
                    }
                }
            }
        }
        
        // === FALLBACK: Jeśli promień 5x5 nie zadziała (np. środek jest dziurą), szukaj wszędzie w pokoju ===
        // (Bardzo rzadki scenariusz, ale daje 100% pewności)
        
        // Przeszukaj cały obszar BoundsInt pokoju
        for (int x = room.xMin + 1; x < room.xMax - 1; x++)
        {
            for (int y = room.yMin + 1; y < room.yMax - 1; y++)
            {
                Vector2Int checkPos = new Vector2Int(x, y);
                if (floorPositions.Contains(checkPos))
                {
                    return checkPos; // Znaleziono jakikolwiek kafelek podłogi w pokoju
                }
            }
        }

        // Oznacza to, że cały BoundsInt pokoju jest ścianami/jest pusty
        return Vector2Int.zero; 
    }

    private Vector2Int FindSafePositionForReward(Vector2Int startPos, List<Vector2Int> forbiddenPositions, HashSet<Vector2Int> floorPositions, BoundsInt room)
    {
        // Szukamy w promieniu 3 kratek od środka
        int maxRadius = 3; 

        for (int r = 1; r <= maxRadius; r++) // Zaczynamy od r=1 (bo r=0 to środek, który jest zajęty)
        {
            for (int x = -r; x <= r; x++)
            {
                for (int y = -r; y <= r; y++)
                {
                    // Sprawdzamy tylko obwód (spiralne szukanie), żeby znaleźć najbliższe miejsce
                    if (Mathf.Abs(x) != r && Mathf.Abs(y) != r) continue;

                    Vector2Int checkPos = startPos + new Vector2Int(x, y);

                    // 1. Musi być na podłodze
                    if (floorPositions.Contains(checkPos))
                    {
                        // 2. Musi być wewnątrz granic pokoju
                        if (room.Contains(new Vector3Int(checkPos.x, checkPos.y, 0)))
                        {
                            // 3. NIE MOŻE być na innej płytce
                            if (!forbiddenPositions.Contains(checkPos))
                            {
                                return checkPos; // Znaleziono wolne miejsce!
                            }
                        }
                    }
                }
            }
        }
        return Vector2Int.zero; // Nie znaleziono miejsca
    }
    public bool TryGetPuzzleDataByRoom(Vector2Int roomKey, out PuzzleData data)
    {
        // activePuzzles to Twój Dictionary<BoundsInt, PuzzleData>
        if (activePuzzles.ContainsKey(roomKey))
        {
            data = activePuzzles[roomKey];
            return true;
        }
        data = null;
        return false;
    }

    private Vector2Int GetRoomCenter(BoundsInt room)
    {
        
        int x = room.xMin + (room.size.x / 2);
        int y = room.yMin + (room.size.y / 2);
        
        return new Vector2Int(x, y);
    }
}