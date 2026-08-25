using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Spawns lightweight floating damage numbers from the shared Health damage event.
/// UI remains event-driven and can later be replaced by a pooled/network-aware presenter.
/// </summary>
public class DamagePopupManager : MonoBehaviour
{
    [SerializeField] private TMP_Text damagePopupPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private float lifetime = 0.8f;
    [SerializeField] private Vector2 randomOffset = new Vector2(0.5f, 0.35f);
    [SerializeField] private int maxActivePopups = 20;

    private readonly Queue<TMP_Text> _activePopups = new();

    private void OnEnable()
    {
        Health.OnDamageTaken += HandleDamageTaken;
    }

    private void OnDisable()
    {
        Health.OnDamageTaken -= HandleDamageTaken;
    }

    private void HandleDamageTaken(Vector3 worldPosition, float damage)
    {
        if (damagePopupPrefab == null || targetCanvas == null || damage <= 0f)
            return;

        Vector2 screenPoint = Camera.main != null
            ? (Vector2)Camera.main.WorldToScreenPoint(worldPosition)
            : worldPosition;

        Vector2 offset = new Vector2(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(-randomOffset.y, randomOffset.y));

        TMP_Text popup = Instantiate(damagePopupPrefab, targetCanvas.transform);
        popup.text = Mathf.CeilToInt(damage).ToString();
        popup.rectTransform.position = screenPoint + offset * 100f;

        _activePopups.Enqueue(popup);
        while (_activePopups.Count > Mathf.Max(1, maxActivePopups))
        {
            TMP_Text oldest = _activePopups.Dequeue();
            if (oldest != null)
                Destroy(oldest.gameObject);
        }

        StartCoroutine(AnimateAndDestroy(popup));
    }

    private System.Collections.IEnumerator AnimateAndDestroy(TMP_Text popup)
    {
        float elapsed = 0f;
        Color startColor = popup.color;
        Vector3 startPosition = popup.rectTransform.position;
        Vector3 endPosition = startPosition + Vector3.up * 60f;

        while (popup != null && elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / lifetime);
            popup.rectTransform.position = Vector3.Lerp(startPosition, endPosition, t);
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, t);
            popup.color = color;
            yield return null;
        }

        if (popup != null)
            Destroy(popup.gameObject);
    }
}
