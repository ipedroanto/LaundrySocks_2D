
// ====================================================================================================================================================================
// Autor: Pedro Antonio Ramos
// Email: ing.ramospedro@gmail.com
// Propósito del script: Controlar el arrastre del imán en pantalla hacia la izquierda, aplicando el achicado visual al sostenerlo. También controla la detección sobre
// la bandeja para limpiar las pelusas y su retorno automático al dock.
// Fecha: 28/09/2026
// ====================================================================================================================================================================

using UnityEngine;
using UnityEngine.EventSystems;

public class MagnetDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool hasTriggeredThisDrag = false;
    private Camera mainCamera;
    private CircleCollider2D myCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        myCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isDragging = true;
        hasTriggeredThisDrag = false;
        transform.localScale = new Vector3(0.85f, 0.85f, 1f); // Achicar imán

        // Notificar que el imán salió del Dock (Bloquea el spawn de nuevas pelusas)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isMagnetActiveInTray = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = -0.2f;

        transform.position = mouseWorldPos;

        if (!hasTriggeredThisDrag)
        {
            CheckMagnetTrigger(mouseWorldPos);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        transform.localScale = Vector3.one;

        ReturnToDock();
    }

    private void CheckMagnetTrigger(Vector3 mouseWorldPos)
    {
        if (myCollider != null) myCollider.enabled = false;

        Collider2D[] hitColliders = Physics2D.OverlapPointAll((Vector2)mouseWorldPos);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null) continue;

            bool isTray = (GameManager.Instance != null && hitCollider.transform == GameManager.Instance.trayTransform) || hitCollider.CompareTag("Tray");

            if (isTray)
            {
                // Limpiar pelusas mediante el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActivateMagnetPowerUp();
                    hasTriggeredThisDrag = true;
                }
                break;
            }
        }

        if (myCollider != null) myCollider.enabled = true;
    }

    public void ReturnToDock()
    {
        transform.localPosition = initialPosition;

        // Notificar que el imán regresó al Dock (Desbloquea el spawn de pelusas)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isMagnetActiveInTray = false;
        }
    }
}