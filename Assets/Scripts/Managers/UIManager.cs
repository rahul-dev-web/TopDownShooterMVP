/// <summary>
/// UIManager - UI System Manager
/// सभी UI screens और HUD को manage करता है
/// 
/// Usage:
/// GameManager.Instance.GetUIManager().ShowScreen("HUD");
/// GameManager.Instance.GetUIManager().UpdateHUDHealth(80);
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private Dictionary<string, GameObject> _screens = new Dictionary<string, GameObject>();
    private string _currentScreenName;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[UIManager] Initializing...");

        // Canvas find करो या create करो
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Future: यहां सभी screens को load करोगे
        // LoadScreen("Splash");
        // LoadScreen("Menu");
        // LoadScreen("Lobby");
        // LoadScreen("HUD");
        // LoadScreen("Settings");
        // LoadScreen("GameOver");

        Debug.Log("[UIManager] ✓ Initialized");
    }

    /// <summary>
    /// Screen को load करता है
    /// </summary>
    public void LoadScreen(string screenName)
    {
        if (_screens.ContainsKey(screenName))
        {
            Debug.Log($"[UIManager] Screen already loaded: {screenName}");
            return;
        }

        // Resources/UI/{screenName} से load करो
        GameObject screenPrefab = Resources.Load<GameObject>($"UI/{screenName}");
        if (screenPrefab == null)
        {
            Debug.LogError($"[UIManager] Screen not found: {screenName}");
            return;
        }

        GameObject screenInstance = Instantiate(screenPrefab);
        screenInstance.name = screenName;
        _screens[screenName] = screenInstance;

        Debug.Log($"[UIManager] Loaded screen: {screenName}");
    }

    /// <summary>
    /// Screen को show करता है
    /// </summary>
    public void ShowScreen(string screenName)
    {
        if (!_screens.ContainsKey(screenName))
        {
            Debug.LogWarning($"[UIManager] Screen not loaded: {screenName}");
            LoadScreen(screenName);
        }

        // पहले current screen को hide करो
        if (!string.IsNullOrEmpty(_currentScreenName) && _screens.ContainsKey(_currentScreenName))
        {
            _screens[_currentScreenName].SetActive(false);
        }

        // नया screen show करो
        _screens[screenName].SetActive(true);
        _currentScreenName = screenName;

        Debug.Log($"[UIManager] Showing screen: {screenName}");
    }

    /// <summary>
    /// Screen को hide करता है
    /// </summary>
    public void HideScreen(string screenName)
    {
        if (!_screens.ContainsKey(screenName))
            return;

        _screens[screenName].SetActive(false);
    }

    // ============== HUD METHODS ==============

    public void UpdateHUDHealth(float currentHealth, float maxHealth)
    {
        // HUD में health bar update करो
        // यह method Phase 4 में implement होगा
    }

    public void UpdateHUDAmmo(int currentAmmo, int maxAmmo)
    {
        // Ammo display update करो
    }

    public void UpdateHUDKills(int kills)
    {
        // Kill counter update करो
    }

    public void UpdateHUDDeaths(int deaths)
    {
        // Death counter update करो
    }

    public void UpdateHUDTimer(float timeRemaining)
    {
        // Match timer update करो
    }

    public void ShowDamagePopup(Vector3 position, int damage)
    {
        // Screen पर damage popup show करो
    }

    public void ShowKillFeed(string killerName, string victimName)
    {
        // Kill feed में entry add करो
    }

    // ============== GETTERS ==============

    public string GetCurrentScreen() => _currentScreenName;

    public bool IsScreenActive(string screenName)
    {
        if (!_screens.ContainsKey(screenName))
            return false;

        return _screens[screenName].activeSelf;
    }

    // ============== CLEANUP ==============

    public void ClearAllScreens()
    {
        foreach (var screen in _screens.Values)
        {
            Destroy(screen);
        }
        _screens.Clear();
        _currentScreenName = string.Empty;
        Debug.Log("[UIManager] Cleared all screens");
    }
}