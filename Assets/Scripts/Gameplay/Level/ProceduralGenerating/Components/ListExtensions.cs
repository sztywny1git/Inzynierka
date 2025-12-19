using System.Collections.Generic;
using UnityEngine; // Używamy UnityEngine.Random

public static class ListExtensions
{
    /// <summary>
    /// Losowo miesza elementy listy, używając algorytmu Fisher-Yates.
    /// Metoda rozszerzająca dla IList<T>.
    /// </summary>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            // Wybiera losowy indeks k od 0 do n (włącznie)
            // Użycie Random.Range(min, max) gdzie max jest ekskluzywne w C# dla int.
            // W Unity (dla int): Random.Range(0, n + 1) to jest poprawny zakres
            int k = Random.Range(0, n + 1); 
            
            // Zamiana elementów
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}