
// ====================================================================================================================================================================
// Autor: Pedro Antonio Ramos
// Email: ing.ramospedro@gmail.com
// Propósito del script: Controlar el comportamiento físico y movimiento flotante de las motas/pelusas en la bandeja, además de bloquear la interacción táctil con las
// medias que se encuentran debajo de las pelusas flotantes.
// Fecha: 09/11/2026
// ====================================================================================================================================================================
using UnityEngine;
using UnityEngine.EventSystems;


public class LintController : MonoBehaviour, IPointerDownHandler
{
    private Vector2 moveDirection;
    private float moveSpeed = 1.0f;

    public void Setup(Vector2 initialVelocity)
    {
        moveDirection = initialVelocity.normalized;
        moveSpeed = initialVelocity.magnitude;
    }

    private void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        // Movimiento flotante continuo dentro de la bandeja
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Rebote en los bordes de la bandeja
        if (Mathf.Abs(transform.localPosition.x) > 2.8f) moveDirection.x *= -1f;
        if (Mathf.Abs(transform.localPosition.y) > 1.5f) moveDirection.y *= -1f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Al tocar la pelusa, bloquea el clic hacia la media de abajo y muestra mensaje
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFeedbackMessage("¡PELUSA EN EL CAMINO!", false);
        }
    }
}