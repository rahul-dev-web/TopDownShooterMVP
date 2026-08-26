#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Safe visual setup helper for the TopDownShooter MVP.
/// It creates only missing scene scaffolding and never deletes existing objects.
/// </summary>
public static class TopDownShooterSceneSetupWizard
{
    private const string MenuRoot = "Tools/TopDownShooter/Scene Setup/";

    [MenuItem(MenuRoot + "Create Missing Gameplay UI")]
    public static void CreateMissingGameplayUI()
    {
        EnsureEventSystem();
        Canvas canvas = EnsureCanvas();
        GameObject hud = EnsureChild(canvas.transform, "HUD");
        EnsureHealthBar(hud.transform);
        EnsureText(hud.transform, "AmmoText", "AMMO 30 / 90", new Vector2(1f, 0f), new Vector2(-24f, -24f), TextAlignmentOptions.Right);
        EnsureText(hud.transform, "TimerText", "05:00", new Vector2(0.5f, 1f), new Vector2(0f, -24f), TextAlignmentOptions.Center);
        EnsureText(hud.transform, "ScoreText", "0 - 0", new Vector2(0.5f, 1f), new Vector2(0f, -64f), TextAlignmentOptions.Center);
        EnsureResultPanel(canvas.transform);

        SaveScene();
        EditorUtility.DisplayDialog("TopDownShooter", "Missing gameplay UI scaffolding was created or preserved. Existing objects were not deleted.", "OK");
    }

    [MenuItem(MenuRoot + "Create Missing Spawn Points")]
    public static void CreateMissingSpawnPoints()
    {
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] all = scene.GetRootGameObjects().SelectMany(GetSelfAndChildren).ToArray();
        if (all.Any(o => o.name.IndexOf("spawn", StringComparison.OrdinalIgnoreCase) >= 0))
        {
            EditorUtility.DisplayDialog("TopDownShooter", "Spawn-like objects already exist. Nothing was changed.", "OK");
            return;
        }

        CreateSpawn("Spawn_Player_A", new Vector3(-4f, 0f, 0f));
        CreateSpawn("Spawn_Player_B", new Vector3(4f, 0f, 0f));
        CreateSpawn("Spawn_Enemy_A", new Vector3(-4f, 3f, 0f));
        CreateSpawn("Spawn_Enemy_B", new Vector3(4f, 3f, 0f));
        SaveScene();
        EditorUtility.DisplayDialog("TopDownShooter", "Four basic spawn point transforms were created. Move them visually to the correct map locations.", "OK");
    }

    [MenuItem(MenuRoot + "Create Basic Scene Helpers")]
    public static void CreateBasicSceneHelpers()
    {
        EnsureMainCamera();
        EnsureEventSystem();
        EnsureNamedRoot("GameManager");
        EnsureNamedRoot("MatchManager");
        EnsureNamedRoot("SpawnManager");
        SaveScene();
        EditorUtility.DisplayDialog("TopDownShooter", "Basic scene helper GameObjects were ensured. This does not add gameplay scripts automatically, so existing script references stay safe.", "OK");
    }

    private static void EnsureMainCamera()
    {
        if (Camera.main != null) return;

        Camera existing = UnityEngine.Object.FindFirstObjectByType<Camera>();
        if (existing != null)
        {
            existing.tag = "MainCamera";
            return;
        }

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void EnsureEventSystem()
    {
        if (UnityEngine.Object.FindFirstObjectByType<EventSystem>() != null) return;

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    private static Canvas EnsureCanvas()
    {
        Canvas existing = UnityEngine.Object.FindFirstObjectByType<Canvas>();
        if (existing != null) return existing;

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static GameObject EnsureChild(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null) return existing.gameObject;

        GameObject child = new GameObject(objectName, typeof(RectTransform));
        child.transform.SetParent(parent, false);
        return child;
    }

    private static void EnsureHealthBar(Transform parent)
    {
        Transform existing = parent.Find("HealthBar");
        if (existing != null) return;

        GameObject health = new GameObject("HealthBar", typeof(RectTransform), typeof(Slider));
        health.transform.SetParent(parent, false);
        RectTransform rect = health.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0f, 0f);
        rect.anchoredPosition = new Vector2(24f, 24f);
        rect.sizeDelta = new Vector2(280f, 32f);

        Slider slider = health.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 100f;
        slider.value = 100f;
    }

    private static void EnsureText(Transform parent, string objectName, string value, Vector2 anchor, Vector2 anchoredPosition, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null) return;

        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(320f, 48f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = 32f;
        text.alignment = alignment;
    }

    private static void EnsureResultPanel(Transform canvas)
    {
        Transform existing = canvas.Find("ResultPanel");
        if (existing != null) return;

        GameObject panel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(720f, 420f);
        panel.SetActive(false);
    }

    private static void CreateSpawn(string objectName, Vector3 position)
    {
        GameObject spawn = new GameObject(objectName);
        spawn.transform.position = position;
    }

    private static GameObject EnsureNamedRoot(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        return existing != null ? existing : new GameObject(objectName);
    }

    private static System.Collections.Generic.IEnumerable<GameObject> GetSelfAndChildren(GameObject root)
    {
        yield return root;
        foreach (Transform child in root.transform)
            foreach (GameObject nested in GetSelfAndChildren(child.gameObject))
                yield return nested;
    }

    private static void SaveScene()
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
#endif
