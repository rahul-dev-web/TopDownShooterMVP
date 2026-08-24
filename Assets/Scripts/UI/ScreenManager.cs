/// <summary>
/// ScreenManager - Central UI screen flow controller.
/// Screens are discovered using the naming convention Screen_<ScreenType>.
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public enum ScreenType
    {
        Splash,
        MainMenu,
        Login,
        Lobby,
        Gameplay,
        Loading,
        Result,
        GameOver,
        Settings,
        Pause
    }

    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.35f;

    private readonly Dictionary<ScreenType, GameObject> _screens = new();
    private ScreenType _currentScreen = ScreenType.MainMenu;
    private Coroutine _transitionRoutine;

    public static event Action<ScreenType> OnScreenChanged;

    private void Awake()
    {
        EnsureFadePanel();
        FindAllScreens();
        ShowInitialScreen();
        Debug.Log($"[ScreenManager] Initialized with {_screens.Count} screens");
    }

    private void EnsureFadePanel()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            return;
        }

        GameObject panel = new GameObject("FadePanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(transform, false);
        panel.transform.SetAsLastSibling();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        fadePanel = panel.GetComponent<CanvasGroup>();
        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private void FindAllScreens()
    {
        Canvas canvas = GetComponent<Canvas>() ?? GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[ScreenManager] No Canvas found.");
            return;
        }

        foreach (Transform child in canvas.transform)
        {
            if (child == fadePanel.transform)
                continue;

            string name = child.gameObject.name;
            if (name.StartsWith("Screen_", StringComparison.Ordinal) &&
                Enum.TryParse(name.Replace("Screen_", string.Empty), out ScreenType screenType))
            {
                _screens[screenType] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }
    }

    private void ShowInitialScreen()
    {
        if (_screens.ContainsKey(ScreenType.Splash))
            _currentScreen = ScreenType.Splash;
        else if (_screens.ContainsKey(ScreenType.MainMenu))
            _currentScreen = ScreenType.MainMenu;

        if (_screens.TryGetValue(_currentScreen, out GameObject screen))
            screen.SetActive(true);

        OnScreenChanged?.Invoke(_currentScreen);
    }

    public void ShowScreen(ScreenType screenType)
    {
        if (!_screens.ContainsKey(screenType))
        {
            Debug.LogWarning($"[ScreenManager] Screen not registered: {screenType}");
            return;
        }

        if (_currentScreen == screenType && _transitionRoutine == null)
            return;

        if (_transitionRoutine != null)
            StopCoroutine(_transitionRoutine);

        _transitionRoutine = StartCoroutine(TransitionToScreen(screenType));
    }

    private IEnumerator TransitionToScreen(ScreenType newScreen)
    {
        fadePanel.blocksRaycasts = true;
        yield return Fade(0f, 1f);

        if (_screens.TryGetValue(_currentScreen, out GameObject current))
            current.SetActive(false);

        _screens[newScreen].SetActive(true);
        _currentScreen = newScreen;
        OnScreenChanged?.Invoke(newScreen);

        yield return Fade(1f, 0f);
        fadePanel.blocksRaycasts = false;
        _transitionRoutine = null;
    }

    private IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            fadePanel.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = to;
    }

    public ScreenType GetCurrentScreen() => _currentScreen;
    public bool HasScreen(ScreenType screenType) => _screens.ContainsKey(screenType);
}