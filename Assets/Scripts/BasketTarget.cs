using System.Collections.Generic;
using UnityEngine;

public class BasketTarget : MonoBehaviour
{
    public int basketIndex;
    public List<string> allowedTypes = new List<string>();
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Setup(int index, string[] types, Color color)
    {
        basketIndex = index;
        allowedTypes = new List<string>(types);
        if (spriteRenderer != null) spriteRenderer.color = color;
    }

    public bool IsTypeAllowed(string typeId)
    {
        // Comparación sin importar mayúsculas/minúsculas ni espacios
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