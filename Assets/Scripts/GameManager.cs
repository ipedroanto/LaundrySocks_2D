using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Referencias de Configuración")]
    public Transform trayTransform;
    public GameObject sockPrefab;
    public GameObject basketPrefab;
    public Transform basketsContainer;

    [Header("Estado del Juego")]
    public int currentLevel = 0;
    public int unlockedLevel = 0;
    public int score = 0;
    public int quotaDone = 0;
    public int streak = 0;
    public float timeLeft = 60f;
    public bool isGameActive = false;

    [Header("Economía y Potenciadores")]
    public int coins = 0;
    public int scoreAccumulatedForCoins = 0;
    public bool hasDirtyBasketUnlocked = false;
    public bool hasMagnetUnlocked = false; // NUEVO: Estado del Imán
    [Range(0f, 1f)]
    public float dirtySockSpawnChance = 0.20f;

    public List<SockType> allSockTypes = new List<SockType>();
    public List<LevelData> levels = new List<LevelData>();

    private List<GameObject> activeSocks = new List<GameObject>();
    private List<BasketTarget> activeBaskets = new List<BasketTarget>();
    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PlayerPrefs.DeleteAll(); // Omitir para mantener guardado entre sesiones
        PlayerPrefs.Save();

        LoadSavedData();
        InitializeData();
    }

    private void LoadSavedData()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        hasDirtyBasketUnlocked = PlayerPrefs.GetInt("HasDirtyBasket", 0) == 1;
        hasMagnetUnlocked = PlayerPrefs.GetInt("HasMagnetUnlocked", 0) == 1;
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        scoreAccumulatedForCoins = PlayerPrefs.GetInt("ScoreAccCoins", 0);
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.SetInt("HasDirtyBasket", hasDirtyBasketUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("HasMagnetUnlocked", hasMagnetUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("UnlockedLevel", unlockedLevel);
        PlayerPrefs.SetInt("ScoreAccCoins", scoreAccumulatedForCoins);
        PlayerPrefs.Save();
    }

    private void InitializeData()
    {
        allSockTypes = new List<SockType>
        {
            new SockType { id = "purple", name = "Morada", color = new Color(0.54f, 0.36f, 1f), basketIndex = 0 },
            new SockType { id = "red", name = "Roja", color = new Color(1f, 0.3f, 0.41f), basketIndex = 1 },
            new SockType { id = "green", name = "Verde", color = new Color(0.24f, 0.84f, 0.55f), basketIndex = 2 },
            new SockType { id = "orange", name = "Naranja", color = new Color(1f, 0.72f, 0.3f), basketIndex = 3 },
            new SockType { id = "blue", name = "Azul", color = new Color(0.16f, 0.48f, 1f), basketIndex = 0 },
            new SockType { id = "yellow", name = "Amarilla", color = new Color(1f, 0.9f, 0.36f), basketIndex = 1 },
            new SockType { id = "teal", name = "Celeste", color = new Color(0.3f, 0.91f, 0.91f), basketIndex = 2 },
            new SockType { id = "pink", name = "Rosa", color = new Color(1f, 0.61f, 0.76f), basketIndex = 3 },
            new SockType { id = "black", name = "Sucia", color = new Color(0.15f, 0.15f, 0.15f), basketIndex = 4 }
        };

        levels = new List<LevelData>
        {
            new LevelData { level = 1, name = "Lavado Suave", types = new[]{"purple","red"}, sockCount = 6, speed = 1.2f, quota = 8, time = 60, desc = "2 tipos, 6 medias" },
            new LevelData { level = 2, name = "Primer Centrifugado", types = new[]{"purple","red","green"}, sockCount = 6, speed = 1.6f, quota = 12, time = 60, desc = "3 tipos" },
            new LevelData { level = 3, name = "Mezcla Media", types = new[]{"purple","red","green","orange"}, sockCount = 6, speed = 2.0f, quota = 15, time = 65, desc = "4 tipos" },
            new LevelData { level = 4, name = "Ropa Color", types = new[]{"purple","red","green","orange","blue"}, sockCount = 6, speed = 2.4f, quota = 18, time = 65, desc = "5 tipos" },
            new LevelData { level = 5, name = "Carga Completa", types = new[]{"purple","red","green","orange","blue","yellow"}, sockCount = 7, speed = 2.8f, quota = 22, time = 70, desc = "6 tipos" },
            new LevelData { level = 6, name = "Express", types = new[]{"purple","red","green","orange","blue","yellow","teal"}, sockCount = 7, speed = 3.4f, quota = 26, time = 70, desc = "7 tipos" },
            new LevelData { level = 7, name = "Delicado", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 8, speed = 3.8f, quota = 30, time = 75, desc = "8 tipos" },
            new LevelData { level = 8, name = "Torbellino", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 8, speed = 4.6f, quota = 32, time = 75, desc = "Muy rápido" },
            new LevelData { level = 9, name = "Lavandería Pro", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 9, speed = 5.4f, quota = 35, time = 80, desc = "9 medias" },
            new LevelData { level = 10, name = "MAESTRO", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 10, speed = 6.2f, quota = 40, time = 85, desc = "Máxima velocidad" }
        };
    }

    public void StartLevel(int levelIdx)
    {
        currentLevel = levelIdx;
        LevelData lvl = levels[currentLevel];

        quotaDone = 0;
        streak = 0;
        timeLeft = lvl.time;
        if (levelIdx == 0) score = 0;

        isGameActive = true;
        ClearScene();
        InitBaskets(lvl.types);
        SpawnSocks(lvl);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD();
            UIManager.Instance.HideOverlay();
        }

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    private void ClearScene()
    {
        foreach (var sock in activeSocks) if (sock != null) Destroy(sock);
        activeSocks.Clear();

        foreach (var b in activeBaskets) if (b != null) Destroy(b.gameObject);
        activeBaskets.Clear();
    }

    private void InitBaskets(string[] levelTypes)
    {
        string[][] basketTypeGroups = new string[][]
        {
            new string[] { "purple", "blue" },
            new string[] { "red", "yellow" },
            new string[] { "green", "teal" },
            new string[] { "orange", "pink" }
        };

        Color[] basketColors = new Color[]
        {
            new Color(0.54f, 0.36f, 1f),
            new Color(1f, 0.3f, 0.41f),
            new Color(0.24f, 0.84f, 0.55f),
            new Color(1f, 0.72f, 0.3f)
        };

        List<int> activeBasketIndices = new List<int>();
        for (int i = 0; i < basketTypeGroups.Length; i++)
        {
            foreach (string t in basketTypeGroups[i])
            {
                if (System.Array.Exists(levelTypes, element => element == t))
                {
                    activeBasketIndices.Add(i);
                    break;
                }
            }
        }

        int count = activeBasketIndices.Count;
        if (hasDirtyBasketUnlocked) count++;

        float spacing = 1.8f;
        float startX = -((count - 1) * spacing) / 2f;

        for (int i = 0; i < activeBasketIndices.Count; i++)
        {
            int bIdx = activeBasketIndices[i];
            GameObject bObj = Instantiate(basketPrefab, basketsContainer);
            bObj.transform.localPosition = new Vector3(startX + (i * spacing), 0, 0);

            BasketTarget target = bObj.GetComponent<BasketTarget>();
            target.Setup(bIdx, basketTypeGroups[bIdx], basketColors[bIdx], false);
            activeBaskets.Add(target);
        }

        if (hasDirtyBasketUnlocked)
        {
            int lastIndex = activeBasketIndices.Count;
            GameObject bObj = Instantiate(basketPrefab, basketsContainer);
            bObj.transform.localPosition = new Vector3(startX + (lastIndex * spacing), 0, 0);

            BasketTarget target = bObj.GetComponent<BasketTarget>();
            target.Setup(4, new string[] { "black" }, Color.white, true);
            activeBaskets.Add(target);
        }
    }

    private void SpawnSocks(LevelData lvl)
    {
        for (int i = 0; i < lvl.sockCount; i++)
        {
            SpawnSingleSock(lvl);
        }
    }

    public void SpawnSingleSock(LevelData lvl)
    {
        SockType sType;

        int blackSocksInTray = GetBlackSocksCount();
        int maxAllowedBlackSocks = lvl.sockCount - 1;

        bool forceCleanSock = false;

        if (!hasDirtyBasketUnlocked)
        {
            if (blackSocksInTray >= maxAllowedBlackSocks || !HasAnyValidSock())
            {
                forceCleanSock = true;
            }
        }

        if (!forceCleanSock && Random.value < dirtySockSpawnChance)
        {
            sType = allSockTypes.Find(t => t.id == "black");
        }
        else
        {
            string randomTypeId = lvl.types[Random.Range(0, lvl.types.Length)];
            sType = allSockTypes.Find(t => t.id == randomTypeId);
        }

        GameObject sObj = Instantiate(sockPrefab, trayTransform);
        float randomX = Random.Range(-2.8f, 2.8f);
        float randomY = Random.Range(-1.5f, 1.5f);
        sObj.transform.localPosition = new Vector3(randomX, randomY, -0.1f);

        SockController controller = sObj.GetComponent<SockController>();
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        controller.Setup(sType, randomDir * lvl.speed);

        activeSocks.Add(sObj);
    }

    private int GetBlackSocksCount()
    {
        int count = 0;
        foreach (GameObject sObj in activeSocks)
        {
            if (sObj != null)
            {
                SockController controller = sObj.GetComponent<SockController>();
                if (controller != null && controller.sockType != null && controller.sockType.id == "black")
                {
                    count++;
                }
            }
        }
        return count;
    }

    private bool HasAnyValidSock()
    {
        foreach (GameObject sObj in activeSocks)
        {
            if (sObj != null)
            {
                SockController controller = sObj.GetComponent<SockController>();
                if (controller != null && controller.sockType != null && controller.sockType.id != "black")
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void OnSockDropped(SockController sock, BasketTarget basket)
    {
        if (!isGameActive) return;

        if (sock.sockType.id == "black" && !basket.isDirtyBasket)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage("MEDIA SUCIA", false);
            }
            sock.ResetToTray();
            return;
        }

        if (basket.IsTypeAllowed(sock.sockType.id))
        {
            int addedQuota = basket.isDirtyBasket ? 2 : 1;
            quotaDone += addedQuota;

            int earnedPoints = 100 + (streak * 10);
            score += earnedPoints;
            scoreAccumulatedForCoins += earnedPoints;
            streak++;

            if (scoreAccumulatedForCoins >= 3000)
            {
                int earnedCoins = scoreAccumulatedForCoins / 3000;
                coins += earnedCoins;
                scoreAccumulatedForCoins %= 3000;
                SaveData();
            }

            string feedbackText = basket.isDirtyBasket ? $"¡SUCIA LIMPIA! +2 CUOTA" : $"¡BIEN! +{earnedPoints}";
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage(feedbackText, true);
            }

            activeSocks.Remove(sock.gameObject);
            Destroy(sock.gameObject);

            StartCoroutine(RespawnSockDelay(0.2f));
            CheckWinCondition();
        }
        else
        {
            streak = 0;
            score = Mathf.Max(0, score - 50);
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage("¡CANASTO EQUIVOCADO!", false);
            }
            sock.ResetToTray();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD();
        }
    }

    // ACCIÓN DEL IMÁN DE CENTRIFUGADO (Auto-Limpieza de medias sucias)
    public void ActivateMagnetPowerUp()
    {
        if (!isGameActive) return;

        List<GameObject> socksToClean = new List<GameObject>();
        foreach (GameObject sObj in activeSocks)
        {
            if (sObj != null)
            {
                SockController controller = sObj.GetComponent<SockController>();
                if (controller != null && controller.sockType != null && controller.sockType.id == "black")
                {
                    socksToClean.Add(sObj);
                }
            }
        }

        if (socksToClean.Count == 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage("¡NO HAY MEDIAS NEGRAS!", false);
            }
            return;
        }

        int cleanedCount = socksToClean.Count;
        foreach (GameObject sObj in socksToClean)
        {
            activeSocks.Remove(sObj);
            Destroy(sObj);
        }

        // Sumar cuota e incremento
        int addedQuota = cleanedCount * 2;
        quotaDone += addedQuota;
        score += cleanedCount * 150;

        if (UIManager.Instance != null)
        {
            //UIManager.Instance.ShowFeedbackMessage($" ¡CENTRIFUGADO! +{addedQuota} CUOTA", true);
            // En GameManager.cs o UIManager.cs:
            UIManager.Instance.ShowFeedbackMessage("<sprite name=\"magnet-131982518836954729_64px\"> ¡CENTRIFUGADO! +2 CUOTA", true);
            UIManager.Instance.UpdateHUD();
        }

        // Reinstanciar las medias faltantes en la escena
        for (int i = 0; i < cleanedCount; i++)
        {
            StartCoroutine(RespawnSockDelay(0.1f * i));
        }

        CheckWinCondition();
    }

    // MÉTODOS DE COMPRA EN TIENDA
    public bool BuyDirtyBasket()
    {
        if (hasDirtyBasketUnlocked) return false;
        if (coins >= 3)
        {
            coins -= 3;
            hasDirtyBasketUnlocked = true;
            SaveData();
            if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
            return true;
        }
        return false;
    }

    public bool BuyMagnetPowerUp()
    {
        if (hasMagnetUnlocked) return false;
        if (coins >= 5)
        {
            coins -= 5;
            hasMagnetUnlocked = true;
            SaveData();
            if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
            return true;
        }
        return false;
    }

    private IEnumerator RespawnSockDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isGameActive) SpawnSingleSock(levels[currentLevel]);
    }

    private void CheckWinCondition()
    {
        if (quotaDone >= levels[currentLevel].quota)
        {
            isGameActive = false;
            if (timerCoroutine != null) StopCoroutine(timerCoroutine);

            if (currentLevel == unlockedLevel && unlockedLevel < 9)
            {
                unlockedLevel++;
            }

            SaveData();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinOverlay();
            }
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
            if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
        }

        isGameActive = false;
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverOverlay();
    }
}