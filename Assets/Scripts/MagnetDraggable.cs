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
        transform.localScale = new Vector3(1.15f, 1.15f, 1f); // Efecto visual de agarrar
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = -0.2f; // Mantener por encima de la bandeja

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

        // Desactivar temporalmente el collider propio para no autodetectarse
        if (myCollider != null) myCollider.enabled = false;

        // Comprobar si se soltó sobre la bandeja principal
        Collider2D hitCollider = Physics2D.OverlapPoint((Vector2)mouseWorldPos);

        if (hitCollider != null)
        {
            // Validar si tocó el transform de la bandeja principal o un objeto con tag/componente de bandeja
            if (hitCollider.transform == GameManager.Instance.trayTransform || hitCollider.CompareTag("Tray"))
            {
                // Disparar la acción del imán en el GameManager
                GameManager.Instance.ActivateMagnetPowerUp();
            }
        }

        // Rehabilitar collider
        if (myCollider != null) myCollider.enabled = true;

        // RETORNO AUTOMÁTICO: Vuelve solo al sitio de donde partió
        ReturnToDock();
    }

    public void ReturnToDock()
    {
        transform.localPosition = initialPosition;
    }
}
