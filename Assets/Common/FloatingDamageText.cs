using UnityEngine;
using TMPro;

public class FloatingDamageText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text textMesh;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Vector2 randomDirectionX = new Vector2(-0.5f, 0.5f);
    [SerializeField] private float initialVerticalBoost = 1f;

    [Header("Fading")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Appearance")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color criticalColor = Color.yellow;
    [SerializeField] private float criticalScaleMultiplier = 1.5f;

    private float _timer;
    private Vector3 _moveDirection;
    private Color _startColor;

    public void Setup(float damageAmount, bool isCritical)
    {
        // 1. Budowanie tekstu (ZMIANA TUTAJ)
        string finalString = Mathf.RoundToInt(damageAmount).ToString();

        if (isCritical)
        {
            finalString += "!"; // Dodajemy wykrzyknik
            
            // Konfiguracja wizualna dla krytyka
            textMesh.color = criticalColor;
            transform.localScale = Vector3.one * criticalScaleMultiplier;
            textMesh.fontStyle = FontStyles.Bold;
        }
        else
        {
            // Konfiguracja dla zwykłego ciosu
            textMesh.color = normalColor;
            transform.localScale = Vector3.one;
            textMesh.fontStyle = FontStyles.Normal;
        }

        // Przypisanie gotowego tekstu do komponentu
        textMesh.text = finalString;

        // Zapamiętanie koloru startowego do zanikania (Alpha)
        _startColor = textMesh.color;

        // 2. Ruch "Popcornu"
        float randomX = Random.Range(randomDirectionX.x, randomDirectionX.y);
        _moveDirection = new Vector3(randomX, initialVerticalBoost, 0).normalized;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        // Ruch
        transform.position += _moveDirection * moveSpeed * Time.deltaTime;

        // Zanikanie (Fade Out)
        if (_timer > lifetime - fadeDuration)
        {
            float fadeProgress = (_timer - (lifetime - fadeDuration)) / fadeDuration;
            float newAlpha = Mathf.Lerp(1f, 0f, fadeProgress);
            textMesh.color = new Color(_startColor.r, _startColor.g, _startColor.b, newAlpha);
        }

        // Zniszczenie
        if (_timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}