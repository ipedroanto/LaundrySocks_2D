using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasketTarget : MonoBehaviour
{
    public int basketIndex; //
    public bool isDirtyBasket = false; //
    public List<string> allowedTypes = new List<string>(); //

    private SpriteRenderer spriteRenderer; //
    private TextMeshPro labelText; //

    private Vector3 originalScale; // Guarda la escala por defecto del canasto
    private Coroutine pulseCoroutine; // Para controlar la animación de pulso

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); //
        labelText = GetComponentInChildren<TextMeshPro>(); //[cite: 1]
        originalScale = transform.localScale; // Guarda la escala inicial del transform
    }

    public void Setup(int index, string[] types, Color color, bool isDirty = false) //[cite: 1]
    {
        basketIndex = index; //[cite: 1]
        allowedTypes = new List<string>(types); //[cite: 1]
        isDirtyBasket = isDirty; //[cite: 1]

        if (isDirty) //[cite: 1]
        {
            if (spriteRenderer != null) spriteRenderer.color = Color.white; //[cite: 1]
            if (labelText != null)
            {
                labelText.text = "LAVADO"; //[cite: 1]
                labelText.color = Color.black; //[cite: 1]
                labelText.gameObject.SetActive(true); //[cite: 1]
            }
        }
        else
        {
            if (spriteRenderer != null) spriteRenderer.color = color; //[cite: 1]
            if (labelText != null)
            {
                labelText.text = ""; //[cite: 1]
                labelText.gameObject.SetActive(false); //[cite: 1]
            }
        }
    }

    public bool IsTypeAllowed(string typeId) //[cite: 1]
    {
        foreach (string t in allowedTypes) //[cite: 1]
        {
            if (string.Equals(t.Trim(), typeId.Trim(), System.StringComparison.OrdinalIgnoreCase)) //[cite: 1]
            {
                return true; //[cite: 1]
            }
        }
        return false; //[cite: 1]
    }

    // NUEVO: Método público que dispara el rebote de tamaño
    public void TriggerPulse()
    {
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    // Corrutina que maneja el crecimiento y encogimiento progresivo
    private IEnumerator PulseRoutine()
    {
        Vector3 targetScale = originalScale * 1.22f; // Crece un 22%
        float duration = 0.08f; // Tiempo de expansión en segundos
        float elapsed = 0f;

        // 1. Agrandar rápidamente
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;
        duration = 0.12f; // Tiempo de retorno a la normalidad

        // 2. Volver al tamaño original suavemente
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalScale;
    }
}