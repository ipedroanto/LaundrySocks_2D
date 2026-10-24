using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

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

        ShowOverlay();
    }

    public void UpdateHUD()
    {
        if (GameManager.Instance == null) return;
        GameManager gm = GameManager.Instance;
        LevelData lvl = gm.levels[gm.currentLevel];

        if (scoreText != null) scoreText.text = gm.score.ToString();
        if (stageText != null) stageText.text = $"{gm.currentLevel + 1}/10";
        if (quotaText != null) quotaText.text = $"{gm.quotaDone}/{lvl.quota}";
        if (timerText != null) timerText.text = $"{gm.timeLeft}s";
        if (streakText != null) streakText.text = $"Racha: {gm.streak}";
        if (coinText != null) coinText.text = $"{gm.coins} monedas        ganadas";

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

    public void ShowOverlay()
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false);

        if (ovTitleText != null) ovTitleText.text = "LAUNDRY SOCKS SORT";
        if (ovDescText != null) ovDescText.text = "Arrastrá cada media al canasto correcto.";

        if (playButton != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null) btnTxt.text = "JUGAR NIVEL 1";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => gm.StartLevel(0));
        }

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
        if (overlayPanel != null) overlayPanel.SetActive(true);
        GameManager gm = GameManager.Instance;

        if (ovTitleText != null)
            ovTitleText.text = $"¡Nivel {gm.currentLevel + 1} Completado!";

        if (ovDescText != null)
            ovDescText.text = $"Puntaje: {gm.score}\nRacha: {gm.streak}";

        if (playButton != null)
        {
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null)
                btnTxt.text = gm.currentLevel < 9 ? $"Nivel {gm.currentLevel + 2}" : "Jugar de Nuevo";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => {
                int nextLvl = gm.currentLevel < 9 ? gm.currentLevel + 1 : 0;
                gm.StartLevel(nextLvl);
            });
        }

        SetupShopPanel();
        UpdateHUD();
        RenderLevelGrid();
    }

    public void ShowGameOverOverlay()
    {
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

    // LÓGICA DE TIENDA NO SECUENCIAL (LIBRE ELECCIÓN)
    private void SetupShopPanel()
    {
        if (shopExchangePanel == null) return;
        shopExchangePanel.SetActive(true);

        GameManager gm = GameManager.Instance;
        if (shopCoinCountText != null)
        {
            shopCoinCountText.text = $"Monedas acumuladas: {gm.coins} (1 moneda x 3000 ptos)";
        }

        // 1. EVALUAR ÍTEM 1: LAVADO (3 🪙)
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

        // 2. EVALUAR ÍTEM 2: IMÁN (5 monedas)
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

        // 3. EVALUAR ÍTEM 3: SUAVIZANTE (1 moneda -> +2s)
        if (buySuavizanteBtn != null)
        {
            TextMeshProUGUI txt = buySuavizanteBtn.GetComponentInChildren<TextMeshProUGUI>();
            bool canAfford = gm.coins >= 1;
            buySuavizanteBtn.interactable = canAfford;

            if (txt != null)
            {
                // Texto con el total acumulado que posee el jugador
                string extraLabel = gm.extraTimeBought > 0 ? $" [Comprado: +{gm.extraTimeBought}s]" : "";

                if (canAfford)
                {
                    int maxPossibleSeconds = gm.coins * 2; // Relación: 1 moneda = +2 segundos
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
                if (gm.BuySuavizanteExtraTime(totalCoinsToSpend)) // Canjea todas las monedas de golpe
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
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    gm.StartLevel(levelIdx);
                });
            }
        }
    }
}