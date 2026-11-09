using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Mecánica de Pelusas (Obstáculo)")]
    public GameObject lintPrefab;                     // Prefab de la pelusa

    public bool isMagnetActiveInTray = false;         // Estado de control del imán
    private List<GameObject> activeLints = new List<GameObject>();
    private Coroutine lintSpawnCoroutine;

    [Header("Mecánica Media Buscada (Wanted Sock)")]
    public GameObject wantedSockContainer;
    public Image wantedSockImage;            // Referencia al Sprite visual de la media buscada
    public GameObject confettiParticlePrefab; // Referencia al prefab ConfettiFX

    private SockType currentWantedSockType;  // Tipo de media buscada en el nivel actual
    private Sprite currentWantedSprite;      // Guarda el sprite/diseño exacto
    private string currentWantedPatternGroup = ""; // CATEGORIA DE PATRON (PLAIN, DOTS, STRIPES)
    private bool isWantedSockClaimed = false; // Control de uso único por nivel

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

    [HideInInspector]
    public List<GameObject> activeSocks = new List<GameObject>();
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
            new SockType { id = "purple", name = "Morada",  color = new Color(0.54f, 0.36f, 1f),   basketIndex = 0 },
            new SockType { id = "red",    name = "Roja",    color = new Color(1f, 0.3f, 0.41f),    basketIndex = 1 },
            new SockType { id = "green",  name = "Verde",   color = new Color(0.24f, 0.84f, 0.55f), basketIndex = 2 },
            new SockType { id = "orange", name = "Naranja", color = new Color(1f, 0.72f, 0.3f),    basketIndex = 3 },
            new SockType { id = "black",  name = "Sucia",   color = new Color(0.353f, 0.298f, 0.184f), basketIndex = 4 }
        };
    

        levels = new List<LevelData>
        {
            new LevelData { level = 1,  name = "Lavado Suave",       types = new[]{"purple","red"},                  sockCount = 6,  speed = 1.2f, quota = 12,  time = 60, desc = "2 colores (Planos)" },
            new LevelData { level = 2,  name = "Primer Centrifugado", types = new[]{"purple","red","green"},          sockCount = 6,  speed = 1.6f, quota = 16, time = 60, desc = "3 colores (Planos)" },
            new LevelData { level = 3,  name = "Mezcla Media",        types = new[]{"purple","red","green","orange"}, sockCount = 6,  speed = 2.0f, quota = 19, time = 65, desc = "4 colores (Planos)" },
            new LevelData { level = 4,  name = "Ropa Color",          types = new[]{"purple","red","green","orange"}, sockCount = 6,  speed = 2.4f, quota = 22, time = 65, desc = "4 colores + Lunares" },
            new LevelData { level = 5,  name = "Carga Completa",     types = new[]{"purple","red","green","orange"}, sockCount = 7,  speed = 2.8f, quota = 24, time = 70, desc = "4 colores + Lunares" },
            new LevelData { level = 6,  name = "Express",             types = new[]{"purple","red","green","orange"}, sockCount = 7,  speed = 3.4f, quota = 29, time = 70, desc = "4 colores + Lunares" },
            new LevelData { level = 7,  name = "Delicado",            types = new[]{"purple","red","green","orange"}, sockCount = 8,  speed = 3.6f, quota = 30, time = 80, desc = "Mezcla Total (Rayas + Lunares)" },
            new LevelData { level = 8,  name = "Torbellino",          types = new[]{"purple","red","green","orange"}, sockCount = 8,  speed = 3.8f, quota = 31, time = 85, desc = "Mezcla Total Rápida" },
            new LevelData { level = 9,  name = "Lavandería Pro",      types = new[]{"purple","red","green","orange"}, sockCount = 9,  speed = 4.2f, quota = 32, time = 90, desc = "9 medias en pantalla" },
            new LevelData { level = 10, name = "MAESTRO",             types = new[]{"purple","red","green","orange"}, sockCount = 10, speed = 4.6f, quota = 34, time = 100, desc = "Máxima velocidad" }
  
        };
    }

    private void Update()
    {
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

        // 2. CHEAT DE SELECCION DE NIVELES: Teclas del 0 al 9
        Key[] numberKeys = new Key[]
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9, Key.Digit0
        };

        for (int i = 0; i < numberKeys.Length; i++)
        {
            if (Keyboard.current[numberKeys[i]].wasPressedThisFrame)
            {
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
        isWantedSockClaimed = false;

        if (AudioManager.Instance != null) AudioManager.Instance.ResetMusicPitch();

        timeLeft = lvl.time + extraTimeBought;
        extraTimeBought = 0;
        SaveData();

        if (levelIdx == 0) score = 0;

        // 1. PRIMERO ACTIVAR EL ESTADO DEL JUEGO
        isGameActive = true;
        isMagnetActiveInTray = false;

        // 2. CANCELAR Y DETENER CORRUTINA DE PELUSAS DEL NIVEL ANTERIOR
        if (lintSpawnCoroutine != null)
        {
            StopCoroutine(lintSpawnCoroutine);
            lintSpawnCoroutine = null;
        }
        // 3. LIMPIAR ESCENA Y PELUSAS PREVIAS
        ClearScene();

        if (magnetDockContainer != null)
            magnetDockContainer.SetActive(hasMagnetUnlocked);

        InitBaskets(lvl.types);
        SpawnSocks(lvl);

        SetupWantedSockForLevel(lvl);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHUD();
            UIManager.Instance.HideOverlay();
        }

        // 4. DETENER CORRUTINAS ANTERIORES
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        // 5. INICIAR TEMPORIZADORES DEL NIVEL
        timerCoroutine = StartCoroutine(TimerRoutine());

        // Iniciar temporizador de pelusas a partir del nivel 3 (levelIdx >= 2)
        if (currentLevel >= 2)
        {
            lintSpawnCoroutine = StartCoroutine(LintSpawnRoutine());
        }
    }

    private void SetupWantedSockForLevel(LevelData lvl)
    {
        isWantedSockClaimed = false;

        List<SockType> validCleanTypes = allSockTypes.FindAll(t => t.id != "black" && System.Array.Exists(lvl.types, element => element == t.id));

        if (validCleanTypes.Count > 0)
        {
            // 1. Elegir tipo de media buscada
            currentWantedSockType = validCleanTypes[Random.Range(0, validCleanTypes.Count)];

            // 2. Obtener sprite y grupo
            currentWantedSprite = GetPatternForLevel(currentLevel + 1);
            currentWantedPatternGroup = GetPatternGroup(currentWantedSprite);

            if (wantedSockContainer != null) wantedSockContainer.SetActive(true);

            if (wantedSockImage != null)
            {
                wantedSockImage.sprite = currentWantedSprite;
                wantedSockImage.color = currentWantedSockType.color;
            }

            Debug.Log($"[WANTED SETUP] Buscada: Color={currentWantedSockType.id}, Grupo={currentWantedPatternGroup}");
        }
        else
        {
            currentWantedPatternGroup = "";
            if (wantedSockContainer != null) wantedSockContainer.SetActive(false);
        }
    }

    private void SpawnSocks(LevelData lvl)
    {
        // 1. Generar la lista de medias normales
        for (int i = 0; i < lvl.sockCount; i++)
        {
            // En la primera media, FORZAMOS que sea exactamente la media buscada
            if (i == 0 && currentWantedSockType != null && !string.IsNullOrEmpty(currentWantedPatternGroup))
            {
                SpawnForcedWantedSock();
            }
            else
            {
                SpawnSingleSock(lvl);
            }
        }
    }

    // Genera una media idéntica a la buscada para asegurar que siempre exista en la bandeja
    private void SpawnForcedWantedSock()
    {
        SockType instanceType = new SockType
        {
            id = currentWantedSockType.id,
            name = currentWantedSockType.name,
            color = currentWantedSockType.color,
            basketIndex = currentWantedSockType.basketIndex,
            customSprite = currentWantedSprite
        };

        GameObject sObj = Instantiate(sockPrefab, trayTransform);
        float randomX = Random.Range(-2.8f, 2.8f);
        float randomY = Random.Range(-1.5f, 1.5f);
        sObj.transform.localPosition = new Vector3(randomX, randomY, -0.1f);

        SockController controller = sObj.GetComponent<SockController>();
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        controller.Setup(instanceType, randomDir * levels[currentLevel].speed);

        activeSocks.Add(sObj);
    }

    // METODO OnSockDropped (Con Canasto Blanco de Lavado)
    public void OnSockDropped(SockController sock, BasketTarget basket)
    {
        if (!isGameActive) return;

        // 1. INTENTO PONER MEDIA SUCIA EN CANASTO NORMAL (ERROR)
        if (sock.sockType.id == "black" && !basket.isDirtyBasket)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrectSFX();
            if (UIManager.Instance != null) UIManager.Instance.ShowFeedbackMessage("MEDIA SUCIA", false);
            sock.ResetToTray();
            return;
        }

        // 2. ACIERTO EN CANASTO CORRECTO O EN CANASTO BLANCO DE LAVADO
        if (basket.IsTypeAllowed(sock.sockType.id))
        {
            bool isWantedMatch = false;

            SpriteRenderer sockSpriteRenderer = sock.GetComponent<SpriteRenderer>();
            Sprite actualSockSprite = (sockSpriteRenderer != null) ? sockSpriteRenderer.sprite : sock.sockType.customSprite;

            string actualPatternGroup = GetPatternGroup(actualSockSprite);

            // Logs para inspección en la Consola de Unity
            if (!isWantedSockClaimed && currentWantedSockType != null)
            {
                bool colorMatch = (sock.sockType.id == currentWantedSockType.id);
                bool patternMatch = (actualPatternGroup == currentWantedPatternGroup);

                Debug.Log($"[EVALUANDO DROP] ColorMatch: {colorMatch} ({sock.sockType.id} vs {currentWantedSockType.id}) | PatternMatch: {patternMatch} ({actualPatternGroup} vs {currentWantedPatternGroup})");

                if (colorMatch && patternMatch)
                {
                    isWantedMatch = true;
                    isWantedSockClaimed = true;
                }
            }

            // Calcular cuota ganada
            int addedQuota = isWantedMatch ? 4 : (basket.isDirtyBasket ? 2 : 1);
            quotaDone += addedQuota;

            int earnedPoints = (basket.isDirtyBasket ? 200 : (100 + (streak * 10))) * (isWantedMatch ? 2 : 1);
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

            // SFX y Particulas
            if (isWantedMatch)
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayWantedBonusSFX();
                if (wantedSockContainer != null) wantedSockContainer.SetActive(false);

                TriggerConfettiFX(basket.transform.position, sock.sockType.color);
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectSFX();
            }

            basket.TriggerPulse();

            if (UIManager.Instance != null)
            {
                string feedbackText = isWantedMatch ? "[BONUS] MEDIA BUSCADA! +4" : (basket.isDirtyBasket ? "SUCIEDAD LAVADA! VALE X 2" : $"BIEN! +{earnedPoints}");
                UIManager.Instance.ShowFeedbackMessage(feedbackText, true);
            }

            activeSocks.Remove(sock.gameObject);
            Destroy(sock.gameObject);

            StartCoroutine(RespawnSockDelay(0.2f));
            CheckWinCondition();
        }
        else
        {
            // 3. CANASTO EQUIVOCADO (ERROR)
            if (AudioManager.Instance != null) AudioManager.Instance.PlayIncorrectSFX();
            streak = 0;
            score = Mathf.Max(0, score - 50);
            if (UIManager.Instance != null) UIManager.Instance.ShowFeedbackMessage("CANASTA EQUIVOCADA!", false);
            sock.ResetToTray();
        }

        if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
    }

    // Clasifica cualquier sprite en su grupo principal (PLAIN, DOTS, STRIPES)
    private string GetPatternGroup(Sprite sprite)
    {
        if (sprite == null || sprite == defaultPlainSprite) return "PLAIN";

        if (dotsPatternSprites != null)
        {
            foreach (Sprite s in dotsPatternSprites)
            {
                if (s != null && s == sprite) return "DOTS";
                if (s != null && sprite != null && s.name == sprite.name) return "DOTS";
            }
        }

        if (stripesPatternSprites != null)
        {
            foreach (Sprite s in stripesPatternSprites)
            {
                if (s != null && s == sprite) return "STRIPES";
                if (s != null && sprite != null && s.name == sprite.name) return "STRIPES";
            }
        }

        return "PLAIN";
    }

    private void TriggerConfettiFX(Vector3 position, Color sockColor)
    {
        if (confettiParticlePrefab == null) return;

        GameObject fxObj = Instantiate(confettiParticlePrefab, position, Quaternion.identity);
        ParticleSystem ps = fxObj.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            var main = ps.main;
            main.startColor = sockColor;
        }

        Destroy(fxObj, 1.5f);
    }

    // Nueva función del Imán: Destruye todas las Pelusas activas de la bandeja
    public void ActivateMagnetPowerUp()
    {
        if (!isGameActive) return;

        if (activeLints.Count == 0)
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage("¡NO HAY PELUSAS!", false);
            }
            return;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayCorrectSFX();

        int cleanedCount = activeLints.Count;

        foreach (GameObject lObj in activeLints)
        {
            if (lObj != null) Destroy(lObj);
        }
        activeLints.Clear();

        score += cleanedCount * 100;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowFeedbackMessage($"¡PELUSAS ELIMINADAS! +{cleanedCount * 100} PTS", true);
            UIManager.Instance.UpdateHUD();
        }
    }
    // CORRUTINA QUE CONTROLADOR EL SPAWN DE PELUSAS
    private IEnumerator LintSpawnRoutine()
    {
        // Pequeña espera inicial para asegurar que la escena cargó completamente
        yield return new WaitForSeconds(1.0f);

        while (isGameActive && currentLevel >= 2)
        {
            // Espera un intervalo aleatorio entre 5 y 9 segundos
            yield return new WaitForSeconds(Random.Range(5f, 9f));

            // SOLO SPAWNEA SI: El imán no se está usando y hay menos de 3 pelusas en pantalla
            if (isGameActive && currentLevel >= 2 && !isMagnetActiveInTray && activeLints.Count < 3)
            {
                SpawnSingleLint();
            }
        }
    }

    // Instancia una pelusa en una posición aleatoria de la bandeja
    private void SpawnSingleLint()
    {
        if (lintPrefab == null || trayTransform == null) return;

        GameObject lObj = Instantiate(lintPrefab, trayTransform);
        float randomX = Random.Range(-2.5f, 2.5f);
        float randomY = Random.Range(-1.2f, 1.2f);
        lObj.transform.localPosition = new Vector3(randomX, randomY, -0.15f);

        LintController controller = lObj.GetComponent<LintController>();
        if (controller != null)
        {
            Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            controller.Setup(randomDir * 1.5f);
        }

        activeLints.Add(lObj);
    }

    private void ClearLints()
    {
        foreach (GameObject lObj in activeLints)
        {
            if (lObj != null) Destroy(lObj);
        }
        activeLints.Clear();
    }

    private void ClearScene()
    {
        foreach (var sock in activeSocks) if (sock != null) Destroy(sock);
        activeSocks.Clear();

        foreach (var b in activeBaskets) if (b != null) Destroy(b.gameObject);
        activeBaskets.Clear();

        // Eliminar pelusas flotantes de la escena
        ClearLints();

        if (wantedSockContainer != null)
        {
            wantedSockContainer.SetActive(false);
        }
    }

    private void InitBaskets(string[] levelTypes)
    {
        string[][] basketTypeGroups = new string[][]
        {
            new string[] { "purple"},
            new string[] { "red"},
            new string[] { "green"},
            new string[] { "orange"}
        };

        Color[] basketColors = new Color[]
        {
            new Color(0.54f, 0.36f, 1f),
            new Color(0.901f, 0.250f, 0.404f),
            new Color(0.211f, 0.709f, 0.545f),
            new Color(0.902f, 0.608f, 0.298f)
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

        // CANASTO BLANCO DE LAVADO
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

    private Sprite GetPatternForLevel(int levelNumber)
    {
        if (levelNumber <= 3)
        {
            return defaultPlainSprite;
        }
        else if (levelNumber <= 6)
        {
            if (Random.value < 0.5f && dotsPatternSprites != null && dotsPatternSprites.Length > 0)
            {
                return dotsPatternSprites[Random.Range(0, dotsPatternSprites.Length)];
            }
            return defaultPlainSprite;
        }
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

    // METODOS DE COMPRA DE LA TIENDA
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

    public bool BuySuavizanteExtraTime(int coinsToSpend)
    {
        if (coinsToSpend <= 0 || coins < coinsToSpend) return false;

        coins -= coinsToSpend;
        extraTimeBought += coinsToSpend * 2;
        SaveData();
        if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
        return true;
    }

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

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.ResetMusicPitch();
            }

            if (currentLevel == unlockedLevel && unlockedLevel < 9)
            {
                unlockedLevel++;
            }

            SaveData();
            ClearScene();

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowWinOverlay();
            }
        }
    }

    private IEnumerator TimerRoutine()
    {
        float warningThreshold = 20f;

        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timeLeft--;

            if (timeLeft <= warningThreshold && AudioManager.Instance != null)
            {
                float progress = 1.0f - (timeLeft / warningThreshold);
                float targetPitch = Mathf.Lerp(1.0f, 1.30f, progress);
                AudioManager.Instance.SetMusicPitch(targetPitch);
            }

            if (UIManager.Instance != null) UIManager.Instance.UpdateHUD();
        }

        isGameActive = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetMusicPitch();
        }

        ClearScene();

        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverOverlay();
    }
}