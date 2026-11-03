using UnityEngine;
using UnityEngine.EventSystems;

public class MagnetDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool hasTriggeredThisDrag = false; // Evita ejecuciones repetidas innecesarias durante el mismo arrastre
    private Camera mainCamera;
    private CircleCollider2D myCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        myCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // Guardar la posición inicial en el Dock a la derecha
        initialPosition = transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isDragging = true;
        hasTriggeredThisDrag = false; // Reiniciar el estado al agarrar el imán
        transform.localScale = new Vector3(1.15f, 1.15f, 1f); // Feedback visual
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // 1. Mover el imán siguiendo la posición del cursor
        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = -0.2f;

        transform.position = mouseWorldPos;

        // 2. ACTIVACIÓN AL MOVER HACIA LA IZQUIERDA (Mientras se arrastra)
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

        // Al soltar el ratón, el imán regresa automáticamente a su dock de origen
        ReturnToDock();
    }

    private void CheckMagnetTrigger(Vector3 mouseWorldPos)
    {
        // Desactivar temporalmente el collider del imán para no autodetectarse
        if (myCollider != null) myCollider.enabled = false;

        // Evaluar todos los colliders bajo la posición actual del imán
        Collider2D[] hitColliders = Physics2D.OverlapPointAll((Vector2)mouseWorldPos);

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null) continue;

            bool isTray = false;

            // Comprobar si está sobre la bandeja principal
            if (GameManager.Instance != null && hitCollider.transform == GameManager.Instance.trayTransform)
            {
                isTray = true;
            }
            else if (hitCollider.CompareTag("Tray"))
            {
                isTray = true;
            }

            if (isTray)
            {
                // Disparar la limpieza de medias sucias en el GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActivateMagnetPowerUp();
                    hasTriggeredThisDrag = true; // Marcar como activado para este movimiento
                }
                break;
            }
        }

        // Reactivar el collider propio
        if (myCollider != null) myCollider.enabled = true;
    }

    public void ReturnToDock()
    {
        transform.localPosition = initialPosition;
    }
}