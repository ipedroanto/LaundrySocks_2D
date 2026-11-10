
// ====================================================================================================================================================================
// Autor: Pedro Antonio Ramos
// Email: ing.ramospedro@gmail.com
// Propósito del script: Controlar la interfaz de usuario completa (HUD superior con fuente Lilita One, barra de progreso, mensajes flotantes, paneles de inicio,
// victoria, game over y la tienda donde se pueden canjear mejoras por monedas).
// Fecha: 09/11/2026
// ====================================================================================================================================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles de Menú y Pantallas")]
    public GameObject mainMenuPanel; // Asigna el MainMenuPanel en el Inspector

    [Header("HUD Principal")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI streakText;
    public TextMeshProUGUI coinText;
    public Image progressBar;
    public TextMeshProUGUI msgText;
    public Button magnetButton;

    [Header("Overlay y Menú")]
    public GameObject overlayPanel;
    public TextMeshProUGUI ovTitleText;
    public TextMeshProUGUI ovDescText;
    public Button playButton;
    public Transform levelGridContainer;
    public GameObject levelButtonPrefab;

    [Header("Panel de Tienda (Libre Selección)")]
    public GameObject shopExchangePanel;
    public TextMeshProUGUI shopCoinCountText;

    [Header("Botones de Ítems Individuales")]
    public Button buyDirtyBasketBtn;  // Botón para LAVADO (3 monedas)
    public Button buyMagnetBtn;       // Botón para IMÁN (5 monedas)
    public Button buySuavizanteBtn;   // Botón para SUAVIZANTE (1 moneda -> +2s)

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (magnetButton != null)
        {
            magnetButton.onClick.RemoveAllListeners();
            magnetButton.onClick.AddListener(() => {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ActivateMagnetPowerUp();
                }
            });
        }

        // CORRECCIÓN 1: Iniciar el juego mostrando la Pantalla 1 (MainMenuPanel)
        ShowMainMenu();
    }

    // PANTALLA 1: Portada Inicial
    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (overlayPanel != null) overlayPanel.SetActive(false);
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false);
        if (GameManager.Instance != null && GameManager.Instance.wantedSockContainer != null)
        {
            GameManager.Instance.wantedSockContainer.SetActive(false);
        }
    }

    // BOTÓN PLAY DE LA PANTALLA 1: Oculta Portada y Abre Pantalla 2 (Overlay + Tienda)
    public void OnPlayButtonPressed()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        // Mostrar la Pantalla 2 con Tienda, Selección de Nivel y Slider de Volumen
        ShowOverlay();
    }

    // BOTÓN DE INICIO EN LA PANTALLA 2: Inicia formalmente el Nivel 1
    public void OnStartLevelButtonPressed()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            // Arrancar en el nivel desbloqueado actual en lugar de forzar siempre el 0
            GameManager.Instance.StartLevel(GameManager.Instance.unlockedLevel);
        }
    }

    public void UpdateHUD()
    {
        if (GameManager.Instance == null) return;
        GameManager gm = GameManager.Instance;
        LevelData lvl = gm.levels[gm.currentLevel];

        // Formateo visual del HUD según referencia (Colores tipográficos en Rich Text)
        if (scoreText != null) 
            scoreText.text = $"<color=#FFD700>{gm.score}</color>";

        if (stageText != null) 
            stageText.text = $"NIVEL: <color=#FFD700>{gm.currentLevel + 1}/{gm.levels.Count}</color>";

        if (quotaText != null) 
            quotaText.text = $"<color=#FFD700>{gm.quotaDone}/{lvl.quota}</color>";

        if (timerText != null) 
            timerText.text = $"<color=#FFD700>{Mathf.CeilToInt(gm.timeLeft)} s</color>";

        if (streakText != null) 
            streakText.text = $"RACHA: <color=#FF8C00>{gm.streak}</color>";

        if (coinText != null) 
            coinText.text = $"<color=#FFD700>{gm.coins}</color>";

        if (progressBar != null)
            progressBar.fillAmount = (float)gm.quotaDone / lvl.quota;

        if (magnetButton != null)
        {
            magnetButton.gameObject.SetActive(gm.isGameActive && gm.hasMagnetUnlocked);
        }
    }

    public void ShowFeedbackMessage(string msg, bool isOk)
    {
        if (msgText == null) return;
        msgText.text = msg;
        msgText.color = isOk ? Color.green : Color.red;
        CancelInvoke(nameof(HideMsg));
        Invoke(nameof(HideMsg), 0.8f);
    }

    private void HideMsg()
    {
        if (msgText != null) msgText.text = "";
    }

    // PANTALLA 2: Tienda, Selección de Nivel y Configuración
    public void ShowOverlay()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false);

        if (GameManager.Instance != null && GameManager.Instance.wantedSockContainer != null)
        {
            GameManager.Instance.wantedSockContainer.SetActive(false);
        }

        if (ovTitleText != null) ovTitleText.text = "LAUNDRY SOCKS SORT";
        if (ovDescText != null) ovDescText.text = "Arrastrá cada media al canasto correcto.";

        if (playButton != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();

            // CORRECCIÓN: Determinar qué nivel arrancar (el desbloqueado actual)
            int targetLevel = gm.unlockedLevel;

            if (btnTxt != null)
                btnTxt.text = $"JUGAR NIVEL {targetLevel + 1}";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                if (overlayPanel != null) overlayPanel.SetActive(false);
                gm.StartLevel(targetLevel); // Carga el nivel desbloqueado actual
            });
        }

        SetupShopPanel();
        UpdateHUD();
        RenderLevelGrid();
    }

    public void HideOverlay()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false);
        UpdateHUD();
    }

    public void ShowWinOverlay()
    {
        if (GameManager.Instance != null && GameManager.Instance.wantedSockContainer != null)
        {
            GameManager.Instance.wantedSockContainer.SetActive(false);
        }

        if (overlayPanel != null) overlayPanel.SetActive(true);
        GameManager gm = GameManager.Instance;

        // Sincronización visual del nivel recién ganado (currentLevel es base 0)
        int levelJustCompleted = gm.currentLevel + 1;
        int nextLevelIndex = gm.currentLevel + 1; // Índice base 0 para el siguiente nivel

        if (ovTitleText != null)
            ovTitleText.text = $"¡Nivel {levelJustCompleted} Completado!";

        if (ovDescText != null)
            ovDescText.text = $"Puntaje: {gm.score}\nRacha: {gm.streak}";

        if (playButton != null)
        {
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            bool isLastLevel = gm.currentLevel >= gm.levels.Count - 1;

            if (btnTxt != null)
                btnTxt.text = !isLastLevel ? $"Nivel {nextLevelIndex + 1}" : "Jugar de Nuevo";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                if (overlayPanel != null) overlayPanel.SetActive(false);

                if (isLastLevel)
                {
                    gm.ResetPlayerProgress();
                    gm.StartLevel(0); // Reiniciar al Nivel 1
                }
                else
                {
                    gm.StartLevel(nextLevelIndex); // Arrancar explícitamente el índice del Nivel 4
                }
            });
        }

        SetupShopPanel();
        UpdateHUD();
        RenderLevelGrid();
    }


    public void ShowGameOverOverlay()
    {
        if (GameManager.Instance != null && GameManager.Instance.wantedSockContainer != null)
        {
            GameManager.Instance.wantedSockContainer.SetActive(false);
        }

        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false);
        GameManager gm = GameManager.Instance;

        if (ovTitleText != null)
            ovTitleText.text = "¡Tiempo!";

        if (ovDescText != null)
            ovDescText.text = $"Llegaste a {gm.quotaDone}/{gm.levels[gm.currentLevel].quota}";

        if (playButton != null)
        {
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null)
                btnTxt.text = $"Reintentar Nivel {gm.currentLevel + 1}";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => gm.StartLevel(gm.currentLevel));
        }

        UpdateHUD();
        RenderLevelGrid();
    }

    private void SetupShopPanel()
    {
        if (shopExchangePanel == null) return;
        shopExchangePanel.SetActive(true);

        GameManager gm = GameManager.Instance;
        if (shopCoinCountText != null)
        {
            shopCoinCountText.text = $"Monedas acumuladas: {gm.coins} (1 moneda x 3000 ptos)";
        }

        if (buyDirtyBasketBtn != null)
        {
            TextMeshProUGUI txt = buyDirtyBasketBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (gm.hasDirtyBasketUnlocked)
            {
                buyDirtyBasketBtn.interactable = false;
                if (txt != null) txt.text = "LAVADO HABILITADO";
            }
            else
            {
                bool canAfford = gm.coins >= 3;
                buyDirtyBasketBtn.interactable = canAfford;
                if (txt != null) txt.text = "LAVADO (3 monedas)";

                buyDirtyBasketBtn.onClick.RemoveAllListeners();
                buyDirtyBasketBtn.onClick.AddListener(() => {
                    if (gm.BuyDirtyBasket()) SetupShopPanel();
                });
            }
        }

        if (buyMagnetBtn != null)
        {
            TextMeshProUGUI txt = buyMagnetBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (gm.hasMagnetUnlocked)
            {
                buyMagnetBtn.interactable = false;
                if (txt != null) txt.text = "IMÁN DISPONIBLE";
            }
            else
            {
                bool canAfford = gm.coins >= 5;
                buyMagnetBtn.interactable = canAfford;
                if (txt != null) txt.text = "IMÁN (5 monedas)";

                buyMagnetBtn.onClick.RemoveAllListeners();
                buyMagnetBtn.onClick.AddListener(() => {
                    if (gm.BuyMagnetPowerUp()) SetupShopPanel();
                });
            }
        }

        if (buySuavizanteBtn != null)
        {
            TextMeshProUGUI txt = buySuavizanteBtn.GetComponentInChildren<TextMeshProUGUI>();
            bool canAfford = gm.coins >= 1;
            buySuavizanteBtn.interactable = canAfford;

            if (txt != null)
            {
                string extraLabel = gm.extraTimeBought > 0 ? $" [Comprado: +{gm.extraTimeBought}s]" : "";

                if (canAfford)
                {
                    int maxPossibleSeconds = gm.coins * 2;
                    txt.text = $"SUAVIZANTE (+{maxPossibleSeconds}s por {gm.coins} monedas){extraLabel}";
                }
                else
                {
                    txt.text = $"SUAVIZANTE (+2s por 1 moneda){extraLabel}";
                }
            }

            buySuavizanteBtn.onClick.RemoveAllListeners();
            buySuavizanteBtn.onClick.AddListener(() => {
                int totalCoinsToSpend = gm.coins;
                if (gm.BuySuavizanteExtraTime(totalCoinsToSpend))
                {
                    SetupShopPanel();
                }
            });
        }
    }

    private void RenderLevelGrid()
    {
        if (GameManager.Instance == null || GameManager.Instance.levels == null) return;
        if (levelGridContainer == null || levelButtonPrefab == null) return;

        foreach (Transform child in levelGridContainer)
        {
            Destroy(child.gameObject);
        }

        GameManager gm = GameManager.Instance;
        for (int i = 0; i < gm.levels.Count; i++)
        {
            int levelIdx = i;
            GameObject btnObj = Instantiate(levelButtonPrefab, levelGridContainer);
            Button btn = btnObj.GetComponent<Button>();
            TextMeshProUGUI txt = btnObj.GetComponentInChildren<TextMeshProUGUI>();

            if (txt != null) txt.text = (i + 1).ToString();

            bool isUnlocked = i <= gm.unlockedLevel;
            if (btn != null)
            {
                btn.interactable = isUnlocked;
                // Cambiar la opacidad/color visual si está bloqueado o desbloqueado
                Image btnImg = btn.GetComponent<Image>();
                if (btnImg != null)
                {
                    Color c = btnImg.color;
                    c.a = isUnlocked ? 1.0f : 0.4f; // Resalta en blanco brillante si se completó/desbloqueó
                    btnImg.color = c;
                }
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (overlayPanel != null) overlayPanel.SetActive(false);
                    gm.StartLevel(levelIdx);
                });
            }
        }
    }
}