using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private List<TextMeshProUGUI> distanceDisplays = new List<TextMeshProUGUI>();
    public GameObject gameOverPanel;
    public TMP_SpriteAsset ticketSpriteAsset;

    [SerializeField] private List<TextMeshProUGUI> ticketDisplays = new List<TextMeshProUGUI>();
    public TextMeshProUGUI finalTicketsText;
    private float distance;
    public int tickets;
    public int totalTickets; // Общее количество билетов
    public ShopManager shopManager;
    public PlayerController playerController;
    public int selectedPlaneId = 0;

    public Canvas MainMenuCanvas;
    public Canvas PauseCanvas;
    public Canvas GameCanvas;
    public Canvas GameOverCanvas;

    private float nextFortuneWheelDistance = 50f;
    private int fortuneWheelCounter = 0;
    private float lastFortuneWheelDistance = 0f;
    public FortuneWheelManager fortuneWheelManager;
    private float distanceSinceLastFortuneWheel = 0f;
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    public GameState currentGameState { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        currentGameState = GameState.MainMenu;
        LoadPlayerData();
    }

    void Start()
    {
        AudioManager.Instance.PlaySound("MenuMusic", musicSource);
        StartScreenManager startScreenManager = FindObjectOfType<StartScreenManager>();
        if (startScreenManager == null || !startScreenManager.startCanvas.enabled)
        {
            SetMainMenuState();
        }
    }

    private void Update()
    {
        if (currentGameState == GameState.Playing)
        {
            if (distance - lastFortuneWheelDistance >= nextFortuneWheelDistance)
            {
                ShowFortuneWheel();
                lastFortuneWheelDistance = distance;
                UpdateNextFortuneWheelDistance();
            }
        }
    }

    public void ShowFortuneWheel()
    {
        fortuneWheelManager.ShowFortuneWheel();
        PauseGame();
    }

    public void AddTickets(int amount)
    {
        totalTickets += amount;
        UpdateAllTicketDisplays();
        SavePlayerData();
    }

    public void SetMainMenuState()
    {
        MainMenuCanvas.enabled = true;
        if (tickets > 0)
        {
            totalTickets += tickets;
        }
        else
        {
        }

        currentGameState = GameState.MainMenu;
        Time.timeScale = 0;

        if (playerController != null)
        {
            playerController.gameObject.SetActive(false);
        }
        else
        {
        }
        AudioManager.Instance.PlaySound("MenuMusic", musicSource);
        MainMenuCanvas.enabled = true;
        PauseCanvas.enabled = false;
        GameCanvas.enabled = false;
        GameOverCanvas.enabled = false;

        UpdateAllTicketDisplays();

        // Сбрасываем дистанцию и билеты текущей игры
        distance = 0f;
        tickets = 0;
        UpdateAllDistanceDisplays();

        SavePlayerData();

    }

    public void UpdateDistance(float newDistance)
    {
        distance = newDistance;
        tickets = Mathf.FloorToInt(distance);
        UpdateAllDistanceDisplays();

        // Add debug log to check distance
    }
    void UpdateAllDistanceDisplays()
    {
        string distanceText = Mathf.FloorToInt(distance) + "m";
        foreach (TextMeshProUGUI display in distanceDisplays)
        {
            if (display != null)
            {
                display.text = distanceText;
            }
        }
    }

    void UpdateAllTicketDisplays()
    {
        string ticketText = totalTickets.ToString() + " <sprite=\"" + ticketSpriteAsset.name + "\" index=0>";
        foreach (TextMeshProUGUI display in ticketDisplays)
        {
            if (display != null)
            {
                display.text = ticketText;
            }
        }
    }
    private void UpdateNextFortuneWheelDistance()
    {
        fortuneWheelCounter++;
        if (fortuneWheelCounter <= 5)
        {
            nextFortuneWheelDistance = 50f * fortuneWheelCounter;
        }
        else
        {
            nextFortuneWheelDistance = 250f;
        }
    }

    public bool CanBuyPlane(int price)
    {
        return totalTickets >= price;
    }

    public void BuyPlane(int planeId, int price)
    {
        totalTickets -= price;
        UpdateAllTicketDisplays();
        shopManager.BuyPlane(planeId);
        SavePlayerData();
    }

    public void AddTicketDisplay(TextMeshProUGUI display)
    {
        if (!ticketDisplays.Contains(display))
        {
            ticketDisplays.Add(display);
            UpdateAllTicketDisplays();
        }
    }

    public void RemoveTicketDisplay(TextMeshProUGUI display)
    {
        ticketDisplays.Remove(display);
    }

    public void AddDistanceDisplay(TextMeshProUGUI display)
    {
        if (!distanceDisplays.Contains(display))
        {
            distanceDisplays.Add(display);
            UpdateAllDistanceDisplays();
        }
    }

    public void RemoveDistanceDisplay(TextMeshProUGUI display)
    {
        distanceDisplays.Remove(display);
    }

    public void SelectPlane(int planeId)
    {
        selectedPlaneId = planeId;
        shopManager.SelectPlane(planeId);
        SavePlayerData();
    }

    public void StartGame()
    {
        currentGameState = GameState.Playing;
        Time.timeScale = 1;

        // Сброс игровых параметров
        lastFortuneWheelDistance = 0f;
        distanceSinceLastFortuneWheel = 0f;
        nextFortuneWheelDistance = 50f;
        fortuneWheelCounter = 0;
        distance = 0f;
        tickets = 0;
        UpdateAllDistanceDisplays();
        UpdateAllTicketDisplays();
        AudioManager.Instance.PlaySound("GameMusic", musicSource);
        // Активация игрока
        if (playerController != null)
        {
            playerController.gameObject.SetActive(true);
            playerController.ResetPosition();
        }
        else
        {
        }

        // Запуск генератора препятствий
        ObstacleGenerator obstacleGenerator = FindObjectOfType<ObstacleGenerator>();
        if (obstacleGenerator != null)
        {
            obstacleGenerator.ResetObstacles();
            obstacleGenerator.StartGenerating();
        }
        else
        {
        }

        // Управление Canvas'ами
        MainMenuCanvas.enabled = false;
        PauseCanvas.enabled = false;
        GameOverCanvas.enabled = false;
        GameCanvas.enabled = true;

    }

    public void RestartGame()
    {
        StartGame();
    }

    public void GameOver()
    {
        currentGameState = GameState.GameOver;
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
        UpdateAllDistanceDisplays();
        if (finalTicketsText != null)
        {
            finalTicketsText.text = tickets + " <sprite=\"" + ticketSpriteAsset.name + "\" index=0>";
        }
        SavePlayerData();
        GameCanvas.enabled = false;
        GameOverCanvas.enabled = true;
        AudioManager.Instance.PlaySound("Crash", sfxSource);
        AudioManager.Instance.PlaySound("GameOverMusic", musicSource);

    }
    public void OnBackToMenuButtonClick()
    {
        AddDistanceTickets();
        SetMainMenuState();
    }
    public void AddDistanceTickets()
    {
        int ticketsToAdd = Mathf.FloorToInt(distance);
        totalTickets += ticketsToAdd;
        UpdateAllTicketDisplays();
        SavePlayerData();
    }
    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            Time.timeScale = 0;
            GameCanvas.enabled = false;
            PauseCanvas.enabled = true;
        }
    }
    public void PlayButtonSound()
    {
        AudioManager.Instance.PlaySound("ButtonClick", sfxSource);
    }
    public void ResumeGame()
    {
        if (currentGameState == GameState.Playing)
        {
            Time.timeScale = 1;
            PauseCanvas.enabled = false;
            GameCanvas.enabled = true;
        }
    }
    void SavePlayerData()
    {
        PlayerPrefs.SetInt("TotalTickets", totalTickets);
        PlayerPrefs.SetInt("SelectedPlaneId", selectedPlaneId);
        PlayerPrefs.Save();
    }

    void LoadPlayerData()
    {
        totalTickets = PlayerPrefs.GetInt("TotalTickets", 0);
        selectedPlaneId = PlayerPrefs.GetInt("SelectedPlaneId", 0);
    }
}