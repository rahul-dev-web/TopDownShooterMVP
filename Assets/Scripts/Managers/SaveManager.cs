/// <summary>
/// SaveManager - Player Data Persistence
/// Player progress, settings, और stats को save/load करता है
/// 
/// Usage:
/// GameManager.Instance.GetSaveManager().SavePlayerData(playerData);
/// PlayerData data = GameManager.Instance.GetSaveManager().LoadPlayerData();
/// </summary>

using UnityEngine;
using System;
using System.IO;

public class SaveManager : MonoBehaviour
{
    private string _savePath;
    private const string PLAYER_DATA_FILE = "PlayerData.json";
    private const string SETTINGS_FILE = "Settings.json";

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[SaveManager] Initializing...");

        // Save path निर्धारित करो
        #if UNITY_ANDROID
            _savePath = Application.persistentDataPath;
        #elif UNITY_IOS
            _savePath = Application.persistentDataPath;
        #else
            _savePath = Path.Combine(Application.persistentDataPath, "Saves");
        #endif

        // Directory create करो अगर exist नहीं करता
        if (!Directory.Exists(_savePath))
        {
            Directory.CreateDirectory(_savePath);
        }

        Debug.Log($"[SaveManager] ✓ Initialized (Path: {_savePath})");
    }

    // ============== PLAYER DATA ==============

    public void SavePlayerData(PlayerData playerData)
    {
        try
        {
            string json = JsonUtility.ToJson(playerData, true);
            string filePath = Path.Combine(_savePath, PLAYER_DATA_FILE);

            File.WriteAllText(filePath, json);
            Debug.Log($"[SaveManager] ✓ Saved player data: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to save player data: {ex.Message}");
        }
    }

    public PlayerData LoadPlayerData()
    {
        try
        {
            string filePath = Path.Combine(_savePath, PLAYER_DATA_FILE);

            if (!File.Exists(filePath))
            {
                Debug.Log("[SaveManager] No save file found, creating default player data");
                return PlayerData.GetDefault();
            }

            string json = File.ReadAllText(filePath);
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);

            Debug.Log("[SaveManager] ✓ Loaded player data");
            return playerData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to load player data: {ex.Message}");
            return PlayerData.GetDefault();
        }
    }

    // ============== SETTINGS ==============

    public void SaveSettings(GameSettings settings)
    {
        try
        {
            string json = JsonUtility.ToJson(settings, true);
            string filePath = Path.Combine(_savePath, SETTINGS_FILE);

            File.WriteAllText(filePath, json);
            Debug.Log($"[SaveManager] ✓ Saved settings: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to save settings: {ex.Message}");
        }
    }

    public GameSettings LoadSettings()
    {
        try
        {
            string filePath = Path.Combine(_savePath, SETTINGS_FILE);

            if (!File.Exists(filePath))
            {
                Debug.Log("[SaveManager] No settings file found, using defaults");
                return GameSettings.GetDefault();
            }

            string json = File.ReadAllText(filePath);
            GameSettings settings = JsonUtility.FromJson<GameSettings>(json);

            Debug.Log("[SaveManager] ✓ Loaded settings");
            return settings;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to load settings: {ex.Message}");
            return GameSettings.GetDefault();
        }
    }

    // ============== UTILITY ==============

    public void DeletePlayerData()
    {
        try
        {
            string filePath = Path.Combine(_savePath, PLAYER_DATA_FILE);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log("[SaveManager] ✓ Deleted player data");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to delete player data: {ex.Message}");
        }
    }

    public void DeleteAllSaveData()
    {
        try
        {
            DeletePlayerData();

            string settingsPath = Path.Combine(_savePath, SETTINGS_FILE);
            if (File.Exists(settingsPath))
            {
                File.Delete(settingsPath);
            }

            Debug.Log("[SaveManager] ✓ Deleted all save data");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] ✗ Failed to delete all save data: {ex.Message}");
        }
    }

    public bool HasSaveData()
    {
        return File.Exists(Path.Combine(_savePath, PLAYER_DATA_FILE));
    }

    public string GetSavePath() => _savePath;
}

// ============== DATA STRUCTURES ==============

[System.Serializable]
public class PlayerData
{
    public string playerID;
    public string playerName;
    public int level;
    public int experience;
    public int kills;
    public int deaths;
    public int matches;
    public int wins;
    public float playTime;
    public int coins;
    public int gems;

    public static PlayerData GetDefault()
    {
        return new PlayerData
        {
            playerID = System.Guid.NewGuid().ToString(),
            playerName = "Player",
            level = 1,
            experience = 0,
            kills = 0,
            deaths = 0,
            matches = 0,
            wins = 0,
            playTime = 0f,
            coins = 100,
            gems = 0
        };
    }

    public override string ToString()
    {
        return $"Player: {playerName} | Level: {level} | K/D: {kills}/{deaths} | Matches: {matches}";
    }
}

[System.Serializable]
public class GameSettings
{
    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;
    public float ambienceVolume;
    public float mouseSensitivity;
    public float joystickDeadzone;
    public bool useJoystick;
    public bool vibrationEnabled;
    public int targetFPS;
    public bool showFPS;
    public string language;
    public bool showBlood;

    public static GameSettings GetDefault()
    {
        return new GameSettings
        {
            masterVolume = 1f,
            musicVolume = 0.7f,
            sfxVolume = 0.8f,
            ambienceVolume = 0.5f,
            mouseSensitivity = 1f,
            joystickDeadzone = 0.1f,
            useJoystick = true,
            vibrationEnabled = true,
            targetFPS = 60,
            showFPS = false,
            language = "en",
            showBlood = true
        };
    }
}