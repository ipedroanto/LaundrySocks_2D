using UnityEngine;
using UnityEngine.EventSystems;

public class MagnetDraggable : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private Vector3 initialPosition;
    private bool isDragging = false;
    private Camera mainCamera;
    private CircleCollider2D myCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        myCollider = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // Guardar la posición inicial en el Dock para poder retornar
        initialPosition = transform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameActive) return;

        isDragging = true;
        transform.localScale = new Vector3(1.15f, 1.15f, 1f); // Feedback de agarrar
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = -0.2f; // Mantener visible sobre los elementos

        transform.position = mouseWorldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        transform.localScale = Vector3.one;

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        if (myCollider != null) myCollider.enabled = false;

        // Obtener TODOS los colliders que están bajo el puntero en ese punto
        Collider2D[] hitColliders = Physics2D.OverlapPointAll((Vector2)mouseWorldPos);

        bool activated = false;

        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider == null) continue;

            bool isTrayHit = false;

            // Comprobar si alguno de los colliders tocados es la bandeja principal
            if (GameManager.Instance != null && hitCollider.transform == GameManager.Instance.trayTransform)
            {
                isTrayHit = true;
            }
            else if (hitCollider.CompareTag("Tray"))
            {
                isTrayHit = true;
            }

            if (isTrayHit)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActivateMagnetPowerUp();
                    activated = true;
                }
                break; // Si ya encontró la bandeja, sale del bucle
            }
        }

        if (myCollider != null) myCollider.enabled = true;

        ReturnToDock();
    }

    public void ReturnToDock()
    {
        transform.localPosition = initialPosition;
    }
}