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
    public TextMeshProUGUI coinText; // NUEVO: Contador de monedas
    public Image progressBar;
    public TextMeshProUGUI msgText;

    [Header("Overlay y Menú")]
    public GameObject overlayPanel;
    public TextMeshProUGUI ovTitleText;
    public TextMeshProUGUI ovDescText;
    public Button playButton;
    public Transform levelGridContainer;
    public GameObject levelButtonPrefab;

    [Header("Panel de Canje (Tienda)")]
    public GameObject shopExchangePanel;
    public TextMeshProUGUI shopInfoText;
    public TextMeshProUGUI shopCoinCountText;
    public Button exchangeButton;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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
        if (coinText != null) coinText.text = $"{gm.coins}";

        if (progressBar != null)
            progressBar.fillAmount = (float)gm.quotaDone / lvl.quota;
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
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false); // Ocultar tienda en menú inicial

        if (ovTitleText != null) ovTitleText.text = "LAUNDRY SOCKS SORT";
        if (ovDescText != null) ovDescText.text = "Arrastrá cada media al canasto correcto.";

        if (playButton != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null) btnTxt.text = $"JUGAR NIVEL 1";

            playButton.onClick.RemoveAllListeners();
           // playButton.onClick.AddListener(() => gm.StartLevel(gm.unlockedLevel));
            playButton.onClick.AddListener(() => gm.StartLevel(0)); // Inicia el Nivel 1 (índice 0)

        }

        UpdateHUD();
        RenderLevelGrid();
    }

    public void HideOverlay()
    {
        if (overlayPanel != null) overlayPanel.SetActive(false);
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
        if (shopExchangePanel != null) shopExchangePanel.SetActive(false); // No mostrar tienda al perder
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
            shopCoinCountText.text = $"Monedas actuales:  {gm.coins}";
        }

        if (gm.hasDirtyBasketUnlocked)
        {
            if (shopInfoText != null) shopInfoText.text = "¡LAVADORA de medias sucias desboqueada!";
            if (exchangeButton != null)
            {
                exchangeButton.interactable = false;
                TextMeshProUGUI btnTxt = exchangeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null) btnTxt.text = "COMPRADO";
            }
        }
        else
        {
            if (shopInfoText != null) shopInfoText.text = "Canjea 3 monedas por una LAVADORA para las medias sucias";

            if (exchangeButton != null)
            {
                bool canAfford = gm.coins >= 3;
                exchangeButton.interactable = canAfford;

                TextMeshProUGUI btnTxt = exchangeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (btnTxt != null) btnTxt.text = canAfford ? "Canje (3)" : "Necesitas 3";

                exchangeButton.onClick.RemoveAllListeners();
                exchangeButton.onClick.AddListener(OnExchangeButtonClicked);
            }
        }
    }

    private void OnExchangeButtonClicked()
    {
        GameManager gm = GameManager.Instance;
        if (gm.BuyDirtyBasket())
        {
            SetupShopPanel();
            UpdateHUD();
        }
    }

    private void RenderLevelGrid()
    {
        if (GameManager.Instance == null || GameManager.Instance.levels == null) return;
        if (levelGridContainer == null || levelButtonPrefab == null) return;

        // Limpiar cuadrícula anterior
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

            if (txt != null)
            {
                txt.text = (i + 1).ToString();
            }

            bool isUnlocked = i <= gm.unlockedLevel;
            if (btn != null)
            {
                btn.interactable = isUnlocked;
                btn.onClick.RemoveAllListeners();
                // Al hacer clic en un número unlocked, inicia ese nivel e inmediatamente cierra el menú
                btn.onClick.AddListener(() => {
                    gm.StartLevel(levelIdx);
                });
            }
        }
    }
}