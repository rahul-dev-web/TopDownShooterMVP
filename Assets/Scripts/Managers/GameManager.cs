/// <summary>
/// GameManager - Core Singleton
/// सभी major systems को initialize aur manage करता है
/// 
/// Usage:
/// GameManager.Instance.GetAudioManager();
/// GameManager.Instance.GetUIManager();
/// </summary>

using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton pattern
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // अगर Scene में GameManager नहीं है, तो create करो
                GameObject managerObject = new GameObject("GameManager");
                _instance = managerObject.AddComponent<GameManager>();
                DontDestroyOnLoad(managerObject);
                Debug.Log("[GameManager] Singleton instance created");
            }
            return _instance;
        }
    }

    // सभी systems का reference
    private AudioManager _audioManager;
    private UIManager _uiManager;
    private InputManager _inputManager;
    private PoolManager _poolManager;
    private SaveManager _saveManager;
    private SettingsManager _settingsManager;
    private MatchManager _matchManager;
    private SpawnManager _spawnManager;

    // Game state
    public enum GameState
    {
        Menu,
        Loading,
        Playing,
        Paused,
        GameOver
    }

    private GameState _currentGameState = GameState.Menu;
    public GameState CurrentGameState => _currentGameState;

    // Events
    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGameInitialized;

    // Debugging
    [SerializeField] private bool debugMode = true;

    // Player Stats
    private int _playerKills = 0;
    private int _playerDeaths = 0;

    private void Awake()
    {
        // Singleton logic
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAllSystems();
        SetGameState(GameState.Menu);
        // OnEnable में या Awake में:
        PoolManager poolManager = GameManager.Instance.GetPoolManager();
        GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Weapons/Bullet");
        poolManager.CreatePool("bullet", bulletPrefab, 100);  // 100 bullets pool
    }

    private void OnEnable()
    {
        Health.OnDeath += HandlePlayerDeath;
        EnemyHealth.OnEnemyKilled += HandleEnemyKilled;

        Debug.Log("[GameManager] Combat events subscribed");
    }

    private void OnDisable()
    {
        Health.OnDeath -= HandlePlayerDeath;
        EnemyHealth.OnEnemyKilled -= HandleEnemyKilled;

        Debug.Log("[GameManager] Combat events unsubscribed");
    }
    private void InitializeAllSystems()
    {
        Debug.Log("[GameManager] Initializing all systems...");

        try
        {
            // Order important है! कुछ managers दूसरों के ऊपर depend करते हैं
            _audioManager = GetOrCreateManager<AudioManager>("AudioManager");
            _inputManager = GetOrCreateManager<InputManager>("InputManager");
            _poolManager = GetOrCreateManager<PoolManager>("PoolManager");
            _saveManager = GetOrCreateManager<SaveManager>("SaveManager");
            _settingsManager = GetOrCreateManager<SettingsManager>("SettingsManager");
            _uiManager = GetOrCreateManager<UIManager>("UIManager");
            _matchManager = GetOrCreateManager<MatchManager>("MatchManager");
            _spawnManager = GetOrCreateManager<SpawnManager>("SpawnManager");

            Debug.Log("[GameManager] ✓ All systems initialized successfully");
            OnGameInitialized?.Invoke();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameManager] ✗ Initialization failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method - Manager को get करता है या create करता है
    /// </summary>
    private T GetOrCreateManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindAnyObjectByType<T>();

        if (manager == null)
        {
            GameObject managerObject = new GameObject(managerName);
            manager = managerObject.AddComponent<T>();
            managerObject.transform.SetParent(transform);
            Debug.Log($"[GameManager] Created {managerName}");
        }
        else
        {
            Debug.Log($"[GameManager] Found existing {managerName}");
        }

        return manager;
    }

    // ============== PUBLIC ACCESSORS ==============

    public AudioManager GetAudioManager() => _audioManager;
    public UIManager GetUIManager() => _uiManager;
    public InputManager GetInputManager() => _inputManager;
    public PoolManager GetPoolManager() => _poolManager;
    public SaveManager GetSaveManager() => _saveManager;
    public SettingsManager GetSettingsManager() => _settingsManager;
    public MatchManager GetMatchManager() => _matchManager;
    public SpawnManager GetSpawnManager() => _spawnManager;

    // ============== GAME STATE ==============

    public void SetGameState(GameState newState)
    {
        if (_currentGameState == newState)
            return;

        GameState previousState = _currentGameState;
        _currentGameState = newState;

        Debug.Log($"[GameManager] Game State Changed: {previousState} → {newState}");
        OnGameStateChanged?.Invoke(newState);

        HandleGameStateChange(newState);
    }

    private void HandleGameStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Menu:
                Time.timeScale = 0f; // Pause game
                break;

            case GameState.Loading:
                Time.timeScale = 0f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f; // Resume game
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                Time.timeScale = 0f;
                break;
        }
    }

    // ============== UTILITY METHODS ==============

    public void PauseGame()
    {
        if (_currentGameState == GameState.Playing)
            SetGameState(GameState.Paused);
    }

    public void ResumeGame()
    {
        if (_currentGameState == GameState.Paused)
            SetGameState(GameState.Playing);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.Loading);
        // अगले frame में scene reload होगा
        Invoke(nameof(ReloadCurrentScene), 0.1f);
    }

    private void ReloadCurrentScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Quitting game...");
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ============== DEBUG ==============

    private void Update()
    {
        if (!debugMode)
            return;

        // Debug keys
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentGameState == GameState.Playing)
                PauseGame();
            else if (_currentGameState == GameState.Paused)
                ResumeGame();
        }
    }

    public void PrintDebugInfo()
    {
        if (!debugMode)
            return;

        Debug.Log("=== GAME DEBUG INFO ===");
        Debug.Log($"Game State: {_currentGameState}");
        Debug.Log($"Time Scale: {Time.timeScale}");
        Debug.Log($"Frame Count: {Time.frameCount}");
        Debug.Log($"FPS: {1f / Time.deltaTime}");
    }

    private void HandlePlayerDeath()
    {
        _playerDeaths++;
        Debug.Log($"[GameManager] Player died. Total Deaths: {_playerDeaths}");
    }

    private void HandleEnemyKilled(int reward)
    {
        _playerKills++;
        Debug.Log($"[GameManager] Enemy killed! Reward: {reward}");
        Debug.Log($"[GameManager] Total Kills: {_playerKills}");
    }

    public int GetPlayerKills()
{
    return _playerKills;
}

    public int GetPlayerDeaths()
{
    return _playerDeaths;
}

    public void ResetStats()
{
    _playerKills = 0;
    _playerDeaths = 0;

    Time.timeScale = 1f; // Reset time scale in case it was paused

    Debug.Log("[GameManager] Stats Reset");
}
}