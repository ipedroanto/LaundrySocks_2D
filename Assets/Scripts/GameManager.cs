using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }



    [Header("NUEVO: Dock del Imán")]
    public GameObject magnetDockContainer; // El objeto MagnetDock completo a la derecha

    [Header("Referencias de Configuración")]
    public Transform trayTransform;
    public GameObject sockPrefab;
    public GameObject basketPrefab;
    public Transform basketsContainer;

    [Header("Sprites de Patrones Visuales")]
    public Sprite defaultPlainSprite;      // Sprite base plano (Columna 1)
    public Sprite[] dotsPatternSprites;    // Sprites con lunares (Columna 2)
    public Sprite[] stripesPatternSprites; // Sprites con rayas (Columna 3)

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
    public bool hasMagnetUnlocked = false;
    public int extraTimeBought = 0; // Segundos extra acumulados por Suavizante

    [Range(0f, 1f)]
    public float dirtySockSpawnChance = 0.30f;

    public List<SockType> allSockTypes = new List<SockType>();
    public List<LevelData> levels = new List<LevelData>();

    private List<GameObject> activeSocks = new List<GameObject>();
    private List<BasketTarget> activeBaskets = new List<BasketTarget>();
    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        PlayerPrefs.DeleteAll(); // Omitir para mantener persistencia si se requiere
        PlayerPrefs.Save();

        LoadSavedData();
        InitializeData();
    }

    private void LoadSavedData()
    {
        coins = PlayerPrefs.GetInt("PlayerCoins", 0);
        hasDirtyBasketUnlocked = PlayerPrefs.GetInt("HasDirtyBasket", 0) == 1;
        hasMagnetUnlocked = PlayerPrefs.GetInt("HasMagnetUnlocked", 0) == 1;
        extraTimeBought = PlayerPrefs.GetInt("ExtraTimeBought", 0);
        unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 0);
        scoreAccumulatedForCoins = PlayerPrefs.GetInt("ScoreAccCoins", 0);
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerCoins", coins);
        PlayerPrefs.SetInt("HasDirtyBasket", hasDirtyBasketUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("HasMagnetUnlocked", hasMagnetUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("ExtraTimeBought", extraTimeBought);
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
            new SockType { id = "yellow", name = "Amarilla", color = new Color(1f, 0.9f, 0.36f), basketIndex = 3 },
            new SockType { id = "teal", name = "Celeste", color = new Color(0.3f, 0.91f, 0.91f), basketIndex = 0 },
            new SockType { id = "pink", name = "Rosa", color = new Color(1f, 0.61f, 0.76f), basketIndex = 1 },
            new SockType { id = "black", name = "Sucia", color = new Color(0.353f, 0.298f, 0.184f), basketIndex = 4 }
        };

        levels = new List<LevelData>
        {
            new LevelData { level = 1, name = "Lavado Suave", types = new[]{"purple","red"}, sockCount = 6, speed = 1.2f, quota = 8, time = 60, desc = "2 tipos, 6 medias" },
            new LevelData { level = 2, name = "Primer Centrifugado", types = new[]{"purple","red","green"}, sockCount = 6, speed = 1.6f, quota = 12, time = 60, desc = "3 tipos" },
            new LevelData { level = 3, name = "Mezcla Media", types = new[]{"purple","red","green","orange"}, sockCount = 6, speed = 2.0f, quota = 15, time = 65, desc = "4 tipos" },
            new LevelData { level = 4, name = "Ropa Color", types = new[]{"purple","red","green","orange","blue"}, sockCount = 6, speed = 2.4f, quota = 18, time = 65, desc = "5 tipos" },
            new LevelData { level = 5, name = "Carga Completa", types = new[]{"purple","red","green","orange","blue","yellow"}, sockCount = 7, speed = 2.8f, quota = 22, time = 70, desc = "6 tipos" },
            new LevelData { level = 6, name = "Express", types = new[]{"purple","red","green","orange","blue","yellow","teal"}, sockCount = 7, speed = 3.4f, quota = 25, time = 70, desc = "7 tipos" },
            new LevelData { level = 7, name = "Delicado", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 8, speed = 3.6f, quota = 26, time = 80, desc = "8 tipos" },
            new LevelData { level = 8, name = "Torbellino", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 8, speed = 3.8f, quota = 28, time = 85, desc = "Muy rápido" },
            new LevelData { level = 9, name = "Lavandería Pro", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 9, speed = 4.0f, quota = 32, time = 90, desc = "9 medias" },
            new LevelData { level = 10, name = "MAESTRO", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 10, speed = 4.2f, quota = 34, time = 95, desc = "Máxima velocidad" }
        };
    }

    private void Update()
    {
        // Verificar que haya un teclado activo
        if (Keyboard.current == null) return;

        // 1. CHEAT DE MONEDAS: Presionar "M" suma 5 monedas
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            coins += 5;
            SaveData();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage("CHEAT: +5 MONEDAS", true);
                UIManager.Instance.UpdateHUD();
            }

            Debug.Log($"[CHEAT] Monedas actuales: {coins}");
        }

        // 2. CHEAT DE SELECCIÓN DE NIVELES: Teclas del 0 al 9
        Key[] numberKeys = new Key[]
        {
        Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
        Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0
        };

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Keyboard.current[numberKeys[i]].wasPressedThisFrame)
            {
                // Mapeo: '1' -> Nivel 1 (idx 0), ..., '9' -> Nivel 9 (idx 8), '0' -> Nivel 10 (idx 9)
                int targetLevelIndex = i;

                if (targetLevelIndex < levels.Count)
                {
                    StartLevel(targetLevelIndex);

                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.ShowFeedbackMessage($"CHEAT: NIVEL {targetLevelIndex + 1}", true);
                    }

                    Debug.Log($"[CHEAT] Salto directo al Nivel {targetLevelIndex + 1}");
                }
            }
        }
    }

    public void StartLevel(int levelIdx)
    {
        currentLevel = levelIdx;
        LevelData lvl = levels[currentLevel];

        quotaDone = 0;
        streak = 0;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetMusicPitch();
        }

        timeLeft = lvl.time + extraTimeBought;
        extraTimeBought = 0;
        SaveData();

        if (levelIdx == 0) score = 0;

        isGameActive = true;
        ClearScene();

        // Mostrar el dock del imán SOLO si el jugador compró el ítem por 5 monedas
        if (magnetDockContainer != null)
        {
            magnetDockContainer.SetActive(hasMagnetUnlocked);
        }

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


    // 3. Actualizar la función de activación del Imán
    public void ActivateMagnetPowerUp()
    {
        if (!isGameActive) return;

        List<GameObject> socksToClean = new List<GameObject>();

        // 1. Recolectar las medias sucias válidas
        for (int i = activeSocks.Count - 1; i >= 0; i--)
        {
            GameObject sObj = activeSocks[i];
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
                UIManager.Instance.ShowFeedbackMessage("¡NO HAY MEDIAS SUCIAS!", false);
            }
            return;
        }

        // 2. Reproducir sonido de acierto
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayCorrectSFX();
        }

        int cleanedCount = socksToClean.Count;

        // 3. Remover de la lista activa y destruir limpiamente
        foreach (GameObject sObj in socksToClean)
        {
            if (sObj != null)
            {
                if (activeSocks.Contains(sObj))
                {
                    activeSocks.Remove(sObj);
                }
                Destroy(sObj);
            }
        }

        // 4. Actualizar puntaje y cuota
        int addedQuota = cleanedCount * 2;
        quotaDone += addedQuota;
        score += cleanedCount * 150;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFeedbackMessage($"¡IMÁN! +{addedQuota} CUOTA", true);
            UIManager.Instance.UpdateHUD();
        }

        // 5. Reinstanciar las medias faltantes en la bandeja
        for (int i = 0; i < cleanedCount; i++)
        {
            StartCoroutine(RespawnSockDelay(0.1f * i));
        }

        CheckWinCondition();
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
            new string[] { "purple", "blue", "teal"},
            new string[] { "red", "pink"},
            new string[] { "green"},
            new string[] { "orange", "yellow"}
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

        // Crear una instancia de SockType asignando el patrón según la progresión del nivel
        SockType instanceType = new SockType
        {
            id = sType.id,
            name = sType.name,
            color = sType.color,
            basketIndex = sType.basketIndex,
            customSprite = GetPatternForLevel(currentLevel + 1)
        };

        GameObject sObj = Instantiate(sockPrefab, trayTransform);
        float randomX = Random.Range(-2.8f, 2.8f);
        float randomY = Random.Range(-1.5f, 1.5f);
        sObj.transform.localPosition = new Vector3(randomX, randomY, -0.1f);

        SockController controller = sObj.GetComponent<SockController>();
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        controller.Setup(instanceType, randomDir * lvl.speed);

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

    // Selecciona el patrón visual (Plano, Lunares o Rayas) según la progresión del nivel
    private Sprite GetPatternForLevel(int levelNumber)
    {
        // Niveles 1 a 3: Solo patrón Plano (Columna 1)
        if (levelNumber <= 3)
        {
            return defaultPlainSprite;
        }
        // Niveles 4 a 6: 50% Plano, 50% Lunares (Columna 2)
        else if (levelNumber <= 6)
        {
            if (Random.value < 0.5f && dotsPatternSprites != null && dotsPatternSprites.Length > 0)
            {
                return dotsPatternSprites[Random.Range(0, dotsPatternSprites.Length)];
            }
            return defaultPlainSprite;
        }
        // Niveles 7 a 10: Combinación de Planas (34%), Lunares (33%) y Rayas (33%)
        else
        {
            float rand = Random.value;
            if (rand < 0.33f && dotsPatternSprites != null && dotsPatternSprites.Length > 0)
            {
                return dotsPatternSprites[Random.Range(0, dotsPatternSprites.Length)];
            }
            else if (rand < 0.66f && stripesPatternSprites != null && stripesPatternSprites.Length > 0)
            {
                return stripesPatternSprites[Random.Range(0, stripesPatternSprites.Length)];
            }
            return defaultPlainSprite;
        }
    }

    public void OnSockDropped(SockController sock, BasketTarget basket)
    {
        if (!isGameActive) return;

    // Intentó poner una media negra en un canasto normal (ERROR)
    if (sock.sockType.id == "black" && !basket.isDirtyBasket)
    {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayIncorrectSFX();
            }

            if (UIManager.Instance != null)
        {
                UIManager.Instance.ShowFeedbackMessage("MEDIA SUCIA", false);
        }
            sock.ResetToTray();
        return;
    }

        // ACIERTO: La media es aceptada en el canasto correcto
        if (basket.IsTypeAllowed(sock.sockType.id))
    {
            // 1. Reproducir sonido de Acierto (Correcto.wav)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCorrectSFX();
            }

            basket.TriggerPulse();

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

            string feedbackText = basket.isDirtyBasket ? $"¡SUCIEDAD LAVADA! VALE X 2 MEDIAS" : $"¡BIEN! +{earnedPoints}";
        if (UIManager.Instance != null)
        {
                UIManager.Instance.ShowFeedbackMessage(feedbackText, true);
        }

            activeSocks.Remove(sock.gameObject);
        Destroy(sock.gameObject);

        StartCoroutine(RespawnSockDelay(0.2f));
        CheckWinCondition();
    }
    // ERROR: Canasto incorrecto
    else
        {
            // 2. Reproducir sonido de Error (Equivocado.wav)
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayIncorrectSFX();
            }

            streak = 0;
        score = Mathf.Max(0, score - 50);
        if (UIManager.Instance != null)
        {
                UIManager.Instance.ShowFeedbackMessage("¡CANASTA EQUIVOCADA!", false);
        }
            sock.ResetToTray();
    }

        if (UIManager.Instance != null)
    {
            UIManager.Instance.UpdateHUD();
    }
    }

 

    // MÉTODOS DE COMPRA DE LA TIENDA
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

    // CANJEAR SUAVIZANTE (1 Moneda = +2 Segundos extra)
    public bool BuySuavizanteExtraTime(int coinsToSpend)
    {
        if (coinsToSpend <= 0 || coins < coinsToSpend) return false;

        coins -= coinsToSpend;
        extraTimeBought += coinsToSpend * 2;
        SaveData();

        if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
        return true;
    }
    // Añadir este método dentro de la clase GameManager en GameManager.cs
    public void ResetPlayerProgress()
    {
        coins = 0;
        scoreAccumulatedForCoins = 0;
        hasDirtyBasketUnlocked = false;
        hasMagnetUnlocked = false;
        extraTimeBought = 0;
        unlockedLevel = 0;

        SaveData();

        if (magnetDockContainer != null)
        {
            magnetDockContainer.SetActive(false);
        }
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

            // Restablecer el pitch a su velocidad normal
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResetMusicPitch();
            }

            if (currentLevel == unlockedLevel && unlockedLevel < 9)
            {
                unlockedLevel++;
            }

            SaveData();

            // Limpiar medias y canastos de la escena para dejar el fondo oscuro limpio
            ClearScene();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinOverlay();
            }
        }
    }

    private IEnumerator TimerRoutine()
    {
        float warningThreshold = 20f; // Tiempo en segundos para empezar a acelerar la música

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timeLeft--;

            // LÓGICA DE ACELERACIÓN DE PITCH
            if (timeLeft <= warningThreshold && AudioManager.Instance != null)
            {
                // Calcula un factor de 0.0 a 1.0 a medida que se acerca a 0
                float progress = 1.0f - (timeLeft / warningThreshold);

                // Acelera progresivamente el pitch desde 1.0x hasta 1.30x (un 30% más rápido)
                float targetPitch = Mathf.Lerp(1.0f, 1.30f, progress);
                AudioManager.Instance.SetMusicPitch(targetPitch);
            }

            if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
        }

        isGameActive = false;

        // Al perder por tiempo, restablecemos el pitch normal
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetMusicPitch();
        }

        ClearScene();

        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverOverlay();

    }

}