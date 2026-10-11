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

    public List<SockType> allSockTypes = new List<SockType>();
    public List<LevelData> levels = new List<LevelData>();

    private List<GameObject> activeSocks = new List<GameObject>();
    private List<BasketTarget> activeBaskets = new List<BasketTarget>();
    private Coroutine timerCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeData(); // Aqui se crean los niveles AQUÍ
    }

    private void InitializeData()
    {
        // 8 Tipos de Medias
        allSockTypes = new List<SockType>
        {
            new SockType { id = "purple", name = "Morada", color = new Color(0.54f, 0.36f, 1f), basketIndex = 0 },
            new SockType { id = "red", name = "Roja", color = new Color(1f, 0.3f, 0.41f), basketIndex = 1 },
            new SockType { id = "green", name = "Verde", color = new Color(0.24f, 0.84f, 0.55f), basketIndex = 2 },
            new SockType { id = "orange", name = "Naranja", color = new Color(1f, 0.72f, 0.3f), basketIndex = 3 },
            new SockType { id = "blue", name = "Azul", color = new Color(0.16f, 0.48f, 1f), basketIndex = 0 },
            new SockType { id = "yellow", name = "Amarilla", color = new Color(1f, 0.9f, 0.36f), basketIndex = 1 },
            new SockType { id = "teal", name = "Celeste", color = new Color(0.3f, 0.91f, 0.91f), basketIndex = 2 },
            new SockType { id = "pink", name = "Rosa", color = new Color(1f, 0.61f, 0.76f), basketIndex = 3 }
        };

        // 10 Niveles idénticos al código JS
        levels = new List<LevelData>
        {
            new LevelData { level = 1, name = "Lavado Suave", types = new[]{"purple","red"}, sockCount = 3, speed = 1.2f, quota = 8, time = 60, desc = "2 tipos, lento" },
            new LevelData { level = 2, name = "Primer Centrifugado", types = new[]{"purple","red","green"}, sockCount = 4, speed = 1.6f, quota = 12, time = 60, desc = "3 tipos" },
            new LevelData { level = 3, name = "Mezcla Media", types = new[]{"purple","red","green","orange"}, sockCount = 5, speed = 2.0f, quota = 15, time = 65, desc = "4 tipos, 4 canastos" },
            new LevelData { level = 4, name = "Ropa Color", types = new[]{"purple","red","green","orange","blue"}, sockCount = 5, speed = 2.4f, quota = 18, time = 65, desc = "5 tipos" },
            new LevelData { level = 5, name = "Carga Completa", types = new[]{"purple","red","green","orange","blue","yellow"}, sockCount = 6, speed = 2.8f, quota = 22, time = 70, desc = "6 tipos" },
            new LevelData { level = 6, name = "Express", types = new[]{"purple","red","green","orange","blue","yellow","teal"}, sockCount = 6, speed = 3.4f, quota = 26, time = 70, desc = "7 tipos, rápido" },
            new LevelData { level = 7, name = "Delicado", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 7, speed = 3.8f, quota = 30, time = 75, desc = "8 tipos" },
            new LevelData { level = 8, name = "Torbellino", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 8, speed = 4.6f, quota = 32, time = 75, desc = "Muy rápido" },
            new LevelData { level = 9, name = "Lavandería Pro", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 9, speed = 5.4f, quota = 35, time = 80, desc = "9 medias a la vez" },
            new LevelData { level = 10, name = "MAESTRO", types = new[]{"purple","red","green","orange","blue","yellow","teal","pink"}, sockCount = 10, speed = 6.2f, quota = 40, time = 85, desc = "10 medias, máxima velocidad!" }
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

        UIManager.Instance.UpdateHUD();
        UIManager.Instance.HideOverlay();

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
        // Definición exacta de grupos de tipos de medias por canasto
        string[][] basketTypeGroups = new string[][]
        {
        new string[] { "purple", "blue" },   // Canasto 0: Morado y Azul
        new string[] { "red", "yellow" },    // Canasto 1: Rojo y Amarillo
        new string[] { "green", "teal" },    // Canasto 2: Verde y Celeste
        new string[] { "orange", "pink" }    // Canasto 3: Naranja y Rosa
        };

        // Colores representativos exactos para cada canasto en la interfaz visual
        Color[] basketColors = new Color[]
        {
        new Color(0.54f, 0.36f, 1f),  // Morado (Canasto 0)
        new Color(1f, 0.3f, 0.41f),   // Rojo (Canasto 1)
        new Color(0.24f, 0.84f, 0.55f),// Verde (Canasto 2)
        new Color(1f, 0.72f, 0.3f)    // Naranja (Canasto 3)
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
        float spacing = 2.2f;
        float startX = -((count - 1) * spacing) / 2f;

        for (int i = 0; i < count; i++)
        {
            int bIdx = activeBasketIndices[i];
            GameObject bObj = Instantiate(basketPrefab, basketsContainer);
            bObj.transform.localPosition = new Vector3(startX + (i * spacing), 0, 0);

            BasketTarget target = bObj.GetComponent<BasketTarget>();
            target.Setup(bIdx, basketTypeGroups[bIdx], basketColors[bIdx]);
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
        string randomTypeId = lvl.types[Random.Range(0, lvl.types.Length)];
        SockType sType = allSockTypes.Find(t => t.id == randomTypeId);

        GameObject sObj = Instantiate(sockPrefab, trayTransform);

        // Posición inicial aleatoria dentro de la bandeja (en escala local 1:1)
        float randomX = Random.Range(-2.8f, 2.8f);
        float randomY = Random.Range(-1.5f, 1.5f);
        sObj.transform.localPosition = new Vector3(randomX, randomY, -0.1f);

        SockController controller = sObj.GetComponent<SockController>();
        Vector2 randomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        controller.Setup(sType, randomDir * lvl.speed);

        activeSocks.Add(sObj);
    }

    public void OnSockDropped(SockController sock, BasketTarget basket)
    {
        if (!isGameActive) return;

        if (basket.IsTypeAllowed(sock.sockType.id))
        {
            // Acierto
            quotaDone++;
            score += 100 + (streak * 10);
            streak++;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowFeedbackMessage($"¡BIEN! +{100 + streak * 10}", true);
            }

            activeSocks.Remove(sock.gameObject);
            Destroy(sock.gameObject);

            StartCoroutine(RespawnSockDelay(0.2f));
            CheckWinCondition();
        }
        else
        {
            // Fallo en canasto equivocado
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

            UIManager.Instance.ShowWinOverlay();
        }
    }

    private IEnumerator TimerRoutine()
    {
        while (timeLeft > 0)
        {
            yield return new WaitForSeconds(1.0f);
            timeLeft--;
            UIManager.Instance.UpdateHUD();
        }

        // Tiempo agotado
        isGameActive = false;
        UIManager.Instance.ShowGameOverOverlay();
    }
}