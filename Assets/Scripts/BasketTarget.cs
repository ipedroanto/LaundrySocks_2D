using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasketTarget : MonoBehaviour
{
    public int basketIndex;
    public bool isDirtyBasket = false;
    public List<string> allowedTypes = new List<string>();

    private SpriteRenderer spriteRenderer;
    private TextMeshPro labelText; // Componente TextMeshPro de mundo 2D

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Busca el componente TextMeshPro en este objeto o en sus hijos
        labelText = GetComponentInChildren<TextMeshPro>();
    }

    public void Setup(int index, string[] types, Color color, bool isDirty = false)
    {
        basketIndex = index;
        allowedTypes = new List<string>(types);
        isDirtyBasket = isDirty;

        if (isDirty)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white;
            }

            if (labelText != null)
            {
                labelText.text = "LAVADO";
                labelText.color = Color.black;
                labelText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            if (labelText != null)
            {
                labelText.text = "";
                labelText.gameObject.SetActive(false);
            }
        }
    }

    public bool IsTypeAllowed(string typeId)
    {
        foreach (string t in allowedTypes)
        {
            if (string.Equals(t.Trim(), typeId.Trim(), System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }
}