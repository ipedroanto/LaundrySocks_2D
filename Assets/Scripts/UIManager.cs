using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI stageText;
    public TextMeshProUGUI quotaText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI streakText;
    public Image progressBar;
    public TextMeshProUGUI msgText;

    [Header("Overlay")]
    public GameObject overlayPanel;
    public TextMeshProUGUI ovTitleText;
    public TextMeshProUGUI ovDescText;
    public Button playButton;
    public Transform levelGridContainer;
    public GameObject levelButtonPrefab;

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
        if (ovTitleText != null) ovTitleText.text = "LAUNDRY SOCKS SORT";
        if (ovDescText != null) ovDescText.text = "Arrastrá cada media según su color al canasto correcto.";

        if (playButton != null && GameManager.Instance != null)
        {
            GameManager gm = GameManager.Instance;
            TextMeshProUGUI btnTxt = playButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnTxt != null) btnTxt.text = $"JUGAR NIVEL {gm.unlockedLevel + 1}";

            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() => gm.StartLevel(gm.unlockedLevel));
        }

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
            ovTitleText.text = $"¡Completaste el nivel {gm.currentLevel + 1}!";

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

        RenderLevelGrid();
    }

    public void ShowGameOverOverlay()
    {
        if (overlayPanel != null) overlayPanel.SetActive(true);
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

        RenderLevelGrid();
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