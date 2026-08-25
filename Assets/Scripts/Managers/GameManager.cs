/// <summary>
/// GameManager - Core singleton and high-level game state owner.
/// </summary>
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject managerObject = new GameObject("GameManager");
                _instance = managerObject.AddComponent<GameManager>();
                DontDestroyOnLoad(managerObject);
            }
            return _instance;
        }
    }

    private AudioManager _audioManager;
    private UIManager _uiManager;
    private InputManager _inputManager;
    private PoolManager _poolManager;
    private SaveManager _saveManager;
    private SettingsManager _settingsManager;
    private MatchManager _matchManager;
    private SpawnManager _spawnManager;

    public enum GameState { Menu, Loading, Playing, Paused, GameOver }
    private GameState _currentGameState = GameState.Menu;
    public GameState CurrentGameState => _currentGameState;

    public static event Action<GameState> OnGameStateChanged;
    public static event Action OnGameInitialized;

    [SerializeField] private bool debugMode = true;
    private int _playerKills;
    private int _playerDeaths;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAllSystems();
        SetGameState(GameState.Menu);

        GameObject bulletPrefab = Resources.Load<GameObject>("Prefabs/Weapons/Bullet");
        if (bulletPrefab != null)
            _poolManager.CreatePool("bullet", bulletPrefab, 100);
        else
            Debug.LogWarning("[GameManager] Bullet prefab not found at Resources/Prefabs/Weapons/Bullet");
    }

    private void OnEnable()
    {
        Health.OnDeath += HandlePlayerDeath;
        EnemyHealth.OnEnemyKilled += HandleEnemyKilled;
    }

    private void OnDisable()
    {
        Health.OnDeath -= HandlePlayerDeath;
        EnemyHealth.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void InitializeAllSystems()
    {
        _audioManager = GetOrCreateManager<AudioManager>("AudioManager");
        _inputManager = GetOrCreateManager<InputManager>("InputManager");
        _poolManager = GetOrCreateManager<PoolManager>("PoolManager");
        _saveManager = GetOrCreateManager<SaveManager>("SaveManager");
        _settingsManager = GetOrCreateManager<SettingsManager>("SettingsManager");
        _uiManager = GetOrCreateManager<UIManager>("UIManager");
        _matchManager = GetOrCreateManager<MatchManager>("MatchManager");
        _spawnManager = GetOrCreateManager<SpawnManager>("SpawnManager");
        OnGameInitialized?.Invoke();
    }

    private T GetOrCreateManager<T>(string managerName) where T : MonoBehaviour
    {
        T manager = FindFirstObjectByType<T>();
        if (manager != null)
            return manager;

        GameObject managerObject = new GameObject(managerName);
        manager = managerObject.AddComponent<T>();
        managerObject.transform.SetParent(transform);
        return manager;
    }

    public AudioManager GetAudioManager() => _audioManager;
    public UIManager GetUIManager() => _uiManager;
    public InputManager GetInputManager() => _inputManager;
    public PoolManager GetPoolManager() => _poolManager;
    public SaveManager GetSaveManager() => _saveManager;
    public SettingsManager GetSettingsManager() => _settingsManager;
    public MatchManager GetMatchManager() => _matchManager;
    public SpawnManager GetSpawnManager() => _spawnManager;

    public void SetGameState(GameState newState)
    {
        if (_currentGameState == newState)
            return;

        _currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
        Time.timeScale = newState == GameState.Playing ? 1f : 0f;
    }

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
        StopAllCoroutines();
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        Time.timeScale = 1f;
        SetGameState(GameState.Loading);
        yield return new WaitForSecondsRealtime(0.1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void Update()
    {
        if (!debugMode)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_currentGameState == GameState.Playing)
                PauseGame();
            else if (_currentGameState == GameState.Paused)
                ResumeGame();
        }
    }

    private void HandlePlayerDeath() => _playerDeaths++;
    private void HandleEnemyKilled(int reward) => _playerKills++;

    public int GetPlayerKills() => _playerKills;
    public int GetPlayerDeaths() => _playerDeaths;

    public void ResetStats()
    {
        _playerKills = 0;
        _playerDeaths = 0;
        Time.timeScale = 1f;
    }
}
