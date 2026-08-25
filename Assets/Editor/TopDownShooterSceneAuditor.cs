#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Editor-only structural and visual setup auditor for the TopDownShooter MVP.
/// Generic component discovery keeps this tool useful while gameplay scripts evolve.
/// </summary>
public static class TopDownShooterSceneAuditor
{
    private const string MenuRoot = "Tools/TopDownShooter/Scene Auditor/";

    [MenuItem(MenuRoot + "Audit Active Scene")]
    public static void AuditActiveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        var report = new StringBuilder();
        var missing = new List<string>();
        var warnings = new List<string>();
        var passes = new List<string>();

        GameObject[] allObjects = GetAllSceneObjects(scene);
        Component[] allComponents = allObjects.SelectMany(o => o.GetComponents<Component>()).Where(c => c != null).ToArray();

        report.AppendLine("TOP DOWN SHOOTER MVP - SCENE AUDIT REPORT");
        report.AppendLine(new string('=', 64));
        report.AppendLine($"Scene: {scene.name}");
        report.AppendLine($"Path: {scene.path}");
        report.AppendLine($"Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"Root Objects: {scene.rootCount}");
        report.AppendLine($"Total GameObjects: {allObjects.Length}");
        report.AppendLine($"Total Components: {allComponents.Length}");

        Section(report, "PHASE 0 - SCENE FOUNDATION");
        CheckRequired(scene.IsValid() && scene.isLoaded, "Active scene is loaded", missing, passes, "Open and save a valid gameplay scene.");
        CheckRequired(Camera.main != null, "Main Camera exists and is tagged MainCamera", missing, passes, "Select gameplay camera and set Tag = MainCamera.");
        CheckRequired(FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null, "EventSystem exists", missing, passes, "Create GameObject > UI > Event System.");

        Section(report, "PHASE 1 - CORE MANAGERS");
        CheckComponentByName(allComponents, new[] { "GameManager" }, "GameManager", missing, warnings, passes, true);
        CheckComponentByName(allComponents, new[] { "MatchManager" }, "MatchManager", missing, warnings, passes, true);
        CheckComponentByName(allComponents, new[] { "SpawnManager" }, "SpawnManager", missing, warnings, passes, true);
        CheckComponentByName(allComponents, new[] { "UIManager" }, "UIManager", missing, warnings, passes, false);
        CheckComponentByName(allComponents, new[] { "AudioManager" }, "AudioManager", missing, warnings, passes, false);
        CheckComponentByName(allComponents, new[] { "PoolManager", "ObjectPool" }, "PoolManager / Object Pool", missing, warnings, passes, false);

        Section(report, "PHASE 2 - PLAYER");
        GameObject player = FindPlayer(allObjects);
        CheckRequired(player != null, "Player GameObject found", missing, passes, "Use Tag = Player, or name the root object Player.");
        if (player != null)
        {
            report.AppendLine($"Player: {GetHierarchyPath(player.transform)}");
            report.AppendLine($"Position: {player.transform.position}");
            report.AppendLine($"Layer: {LayerMask.LayerToName(player.layer)} ({player.layer})");
            report.AppendLine($"Tag: {player.tag}");
            CheckRequired(player.GetComponent<Rigidbody2D>() != null, "Player has Rigidbody2D", missing, passes, "Add Rigidbody2D to the Player root.");
            CheckRequired(player.GetComponent<Collider2D>() != null || player.GetComponentInChildren<Collider2D>(true) != null, "Player has Collider2D", missing, passes, "Add a Collider2D to Player or a child.");
            CheckComponentByName(player.GetComponentsInChildren<Component>(true).Where(c => c != null).ToArray(), new[] { "PlayerController", "PlayerMovement", "PlayerMovementController" }, "Player movement controller", missing, warnings, passes, true);
        }

        Section(report, "PHASE 3 - WEAPONS");
        CheckComponentByName(allComponents, new[] { "WeaponController", "WeaponManager", "GunController" }, "Weapon controller", missing, warnings, passes, true);
        CheckComponentByName(allComponents, new[] { "DamageDealer", "Projectile", "Bullet" }, "Damage / projectile system", missing, warnings, passes, true);
        int firePoints = allObjects.Count(o => ContainsIgnoreCase(o.name, "firepoint") || ContainsIgnoreCase(o.name, "muzzle"));
        CheckWarning(firePoints > 0, "FirePoint / Muzzle object found", warnings, passes, "Create a child transform at the weapon barrel and name it FirePoint.");

        Section(report, "PHASE 4 - COMBAT");
        CheckComponentByName(allComponents, new[] { "Health" }, "Health component", missing, warnings, passes, true);
        CheckComponentByName(allComponents, new[] { "HealPickup" }, "Heal pickup", missing, warnings, passes, false);
        CheckComponentByName(allComponents, new[] { "KillFeedDisplay" }, "Kill feed display", missing, warnings, passes, false);
        CheckComponentByName(allComponents, new[] { "DamagePopupManager" }, "Damage popup manager", missing, warnings, passes, false);
        int spawnPoints = allObjects.Count(o => ContainsIgnoreCase(o.name, "spawn"));
        CheckRequired(spawnPoints > 0, "At least one spawn point found", missing, passes, "Create empty objects for spawn locations and include Spawn in their names.");
        report.AppendLine($"Spawn-like objects found: {spawnPoints}");

        Section(report, "PHASE 5 - UI");
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        CheckRequired(canvases.Length > 0, "Canvas exists", missing, passes, "Create GameObject > UI > Canvas.");
        CheckWarning(FindNamedObject(allObjects, "HUD") != null || HasComponentName(allComponents, "HUD"), "HUD object/component found", warnings, passes, "Create a HUD root under Canvas.");
        CheckWarning(FindNamedObject(allObjects, "Result") != null, "Result screen/panel found", warnings, passes, "Create a Result panel under Canvas.");
        CheckWarning(FindNamedObject(allObjects, "Health") != null || allComponents.Any(c => c is Slider), "Health UI candidate found", warnings, passes, "Add a health bar and name its object HealthBar.");
        CheckWarning(FindNamedObject(allObjects, "Ammo") != null, "Ammo UI candidate found", warnings, passes, "Add an ammo text element and name it AmmoText.");
        CheckWarning(FindNamedObject(allObjects, "Timer") != null, "Timer UI candidate found", warnings, passes, "Add a timer text element and name it TimerText.");
        CheckWarning(FindNamedObject(allObjects, "Score") != null, "Score UI candidate found", warnings, passes, "Add a score text element and name it ScoreText.");

        Section(report, "VISUAL SETUP SNAPSHOT - ROOT OBJECTS");
        foreach (GameObject root in scene.GetRootGameObjects())
            report.AppendLine($"- {root.name} | pos={root.transform.position} | active={root.activeInHierarchy}");

        report.AppendLine();
        report.AppendLine("RESULTS");
        report.AppendLine($"PASS: {passes.Count}");
        report.AppendLine($"WARNING: {warnings.Count}");
        report.AppendLine($"MISSING / REQUIRED: {missing.Count}");

        if (missing.Count > 0)
        {
            report.AppendLine("\nREQUIRED FIXES");
            foreach (string item in missing.Distinct()) report.AppendLine("[MISSING] " + item);
        }
        if (warnings.Count > 0)
        {
            report.AppendLine("\nRECOMMENDED / OPTIONAL CHECKS");
            foreach (string item in warnings.Distinct()) report.AppendLine("[WARNING] " + item);
        }

        int score = Mathf.Clamp(Mathf.RoundToInt(100f * passes.Count / Mathf.Max(1, passes.Count + warnings.Count + missing.Count)), 0, 100);
        report.AppendLine($"\nSCENE READINESS SCORE: {score}%");
        report.AppendLine("This is a structural readiness score, not a runtime gameplay guarantee.");

        string reportText = report.ToString();
        Debug.Log(reportText);
        WriteReportToProject(reportText);
        EditorUtility.DisplayDialog("Scene Audit Complete", $"Readiness: {score}%\nPass: {passes.Count}\nWarnings: {warnings.Count}\nMissing: {missing.Count}\n\nReport: SceneAuditReports/latest-scene-audit.txt", "OK");
    }

    [MenuItem(MenuRoot + "Select Main Camera")]
    private static void SelectMainCamera()
    {
        if (Camera.main == null) { EditorUtility.DisplayDialog("Scene Auditor", "No Main Camera found.", "OK"); return; }
        Selection.activeGameObject = Camera.main.gameObject;
        EditorGUIUtility.PingObject(Camera.main.gameObject);
    }

    [MenuItem(MenuRoot + "Open Latest Report")]
    private static void OpenLatestReport()
    {
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SceneAuditReports", "latest-scene-audit.txt");
        if (!File.Exists(path)) { EditorUtility.DisplayDialog("Scene Auditor", "No audit report exists yet. Run Audit Active Scene first.", "OK"); return; }
        EditorUtility.RevealInFinder(path);
    }

    private static void Section(StringBuilder report, string title) { report.AppendLine(); report.AppendLine(title); report.AppendLine(new string('-', 48)); }
    private static void CheckRequired(bool condition, string label, List<string> missing, List<string> passes, string fix) { if (condition) passes.Add(label); else missing.Add(label + " -> Fix: " + fix); }
    private static void CheckWarning(bool condition, string label, List<string> warnings, List<string> passes, string fix) { if (condition) passes.Add(label); else warnings.Add(label + " -> Recommendation: " + fix); }

    private static void CheckComponentByName(Component[] components, string[] acceptedNames, string label, List<string> missing, List<string> warnings, List<string> passes, bool required)
    {
        bool found = components.Any(c => acceptedNames.Any(n => string.Equals(c.GetType().Name, n, StringComparison.OrdinalIgnoreCase)));
        string fix = "Expected component: " + string.Join(" or ", acceptedNames) + ".";
        if (found) passes.Add(label); else if (required) missing.Add(label + " -> Fix: " + fix); else warnings.Add(label + " -> Recommendation: " + fix);
    }

    private static GameObject FindPlayer(GameObject[] objects)
    {
        GameObject tagged = objects.FirstOrDefault(o => o.CompareTag("Player"));
        return tagged != null ? tagged : objects.FirstOrDefault(o => string.Equals(o.name, "Player", StringComparison.OrdinalIgnoreCase));
    }
    private static GameObject FindNamedObject(GameObject[] objects, string namePart) => objects.FirstOrDefault(o => ContainsIgnoreCase(o.name, namePart));
    private static bool HasComponentName(Component[] components, string namePart) => components.Any(c => ContainsIgnoreCase(c.GetType().Name, namePart));
    private static bool ContainsIgnoreCase(string value, string part) => value != null && value.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0;
    private static GameObject[] GetAllSceneObjects(Scene scene) => scene.GetRootGameObjects().SelectMany(GetSelfAndChildren).ToArray();
    private static IEnumerable<GameObject> GetSelfAndChildren(GameObject root) { yield return root; foreach (Transform child in root.transform) foreach (GameObject nested in GetSelfAndChildren(child.gameObject)) yield return nested; }
    private static string GetHierarchyPath(Transform transform) { var names = new List<string>(); while (transform != null) { names.Add(transform.name); transform = transform.parent; } names.Reverse(); return string.Join("/", names); }
    private static void WriteReportToProject(string text) { string root = Directory.GetParent(Application.dataPath).FullName; string directory = Path.Combine(root, "SceneAuditReports"); Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, "latest-scene-audit.txt"), text); }
}
#endif
