using UnityEngine;
using UnityEngine.EventSystems;

public class SockController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public SockType sockType;
    private Vector2 velocity;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Camera mainCamera;

    private float minX = -3.0f, maxX = 3.0f;
    private float minY = -1.8f, maxY = 1.8f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        mainCamera = Camera.main;
    }

    public void Setup(SockType type, Vector2 initialVelocity)
    {
        sockType = type;
        velocity = initialVelocity;
        if (spriteRenderer != null && type != null)
        {
            spriteRenderer.color = type.color;
        }
    }

    private void Update()
    {
        if (isDragging || !GameManager.Instance.isGameActive) return;

        transform.Translate(velocity * Time.deltaTime, Space.World);

        Vector3 pos = transform.localPosition;
        if (pos.x < minX || pos.x > maxX)
        {
            velocity.x *= -1;
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
        }
        if (pos.y < minY || pos.y > maxY)
        {
            velocity.y *= -1;
            pos.y = Mathf.Clamp(pos.y, minY, maxY);
        }
        transform.localPosition = pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GameManager.Instance.isGameActive) return;

        isDragging = true;
        transform.localScale = new Vector3(0.6f, 0.6f, 1f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = -0.1f;

        transform.position = mouseWorldPos;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        Vector3 mouseScreenPos = eventData.position;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);

        // 1. Apagar temporalmente el collider de la media para que no se autodetecte
        if (circleCollider != null) circleCollider.enabled = false;

        // 2. Hacer la detección en la posición donde se soltó la media
        Collider2D hitCollider = Physics2D.OverlapPoint((Vector2)mouseWorldPos);

        if (hitCollider != null)
        {
            Debug.Log($"Objeto tocado: {hitCollider.name} | Tag: {hitCollider.tag}");

            if (hitCollider.CompareTag("Basket"))
            {
                BasketTarget basket = hitCollider.GetComponent<BasketTarget>();
                if (basket != null)
                {
                    // Volver a encender el collider antes de procesar entrega o destrucción
                    if (circleCollider != null) circleCollider.enabled = true;
                    GameManager.Instance.OnSockDropped(this, basket);
                    return;
                }
            }
        }

        // 3. Volver a encender el collider si no cayó en un canasto y regresa a la bandeja
        if (circleCollider != null) circleCollider.enabled = true;
        ResetToTray();
    }

    public void ResetToTray()
    {
        Vector3 pos = transform.localPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        pos.z = 0;
        transform.localPosition = pos;
    }
}