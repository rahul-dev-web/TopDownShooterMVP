/// <summary>
/// ScreenManager - Manages all UI screens
/// 
/// Screen transitions को handle करता है
/// Active screen track करता है
/// Fade effects manage करता है
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using System;

public class ScreenManager : MonoBehaviour
{
    // Screen types
    public enum ScreenType
    {
        MainMenu,
        Gameplay,
        GameOver,
        Settings,
        Pause
    }

    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private float fadeDuration = 0.5f;

    private Dictionary<ScreenType, GameObject> _screens = new Dictionary<ScreenType, GameObject>();
    private ScreenType _currentScreen = ScreenType.MainMenu;
    private CanvasGroup _fadePanelGroup;

    // Events
    public static event Action<ScreenType> OnScreenChanged;

    private void Awake()
    {
        if (fadePanel == null)
        {
            // Create fade panel if not assigned
            GameObject fadePanelObj = new GameObject("FadePanel");
            fadePanelObj.transform.SetParent(transform);
            fadePanel = fadePanelObj.AddComponent<CanvasGroup>();
            _fadePanelGroup = fadePanel;
        }
        else
        {
            _fadePanelGroup = fadePanel.GetComponent<CanvasGroup>();
        }

        FindAllScreens();
        Debug.Log("[ScreenManager] Initialized");
    }

    private void FindAllScreens()
    {
        // Find all screen GameObjects
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
        {
            foreach (Transform child in canvas.transform)
            {
                // Assume naming convention: "Screen_MainMenu", "Screen_GameOver", etc.
                string screenName = child.gameObject.name;
                if (Enum.TryParse<ScreenType>(screenName.Replace("Screen_", ""), out ScreenType screenType))
                {
                    _screens[screenType] = child.gameObject;
                    child.gameObject.SetActive(screenType == ScreenType.MainMenu); // Show only MainMenu at start
                }
            }
        }

        Debug.Log($"[ScreenManager] Found {_screens.Count} screens");
    }

    public void ShowScreen(ScreenType screenType)
    {
        if (_currentScreen == screenType)
            return;

        StartCoroutine(TransitionToScreen(screenType));
    }

    private System.Collections.IEnumerator TransitionToScreen(ScreenType newScreen)
    {
        // Fade out current screen
        yield return StartCoroutine(FadeIn());

        // Hide current screen
        if (_screens.ContainsKey(_currentScreen))
        {
            _screens[_currentScreen].SetActive(false);
        }

        // Show new screen
        if (_screens.ContainsKey(newScreen))
        {
            _screens[newScreen].SetActive(true);
        }

        _currentScreen = newScreen;
        OnScreenChanged?.Invoke(newScreen);

        // Fade in new screen
        yield return StartCoroutine(FadeOut());

        Debug.Log($"[ScreenManager] Transitioned to {newScreen}");
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadePanelGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        _fadePanelGroup.alpha = 1f;
    }

    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            _fadePanelGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        _fadePanelGroup.alpha = 0f;
    }

    public ScreenType GetCurrentScreen() => _currentScreen;
}