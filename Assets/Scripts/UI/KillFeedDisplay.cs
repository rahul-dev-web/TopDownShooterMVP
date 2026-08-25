using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Renders recent kill events as a simple rolling kill feed.
/// MatchManager remains the source of match state; this component only presents events.
/// </summary>
public class KillFeedDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text killFeedEntryPrefab;
    [SerializeField] private Transform container;
    [SerializeField, Min(0.5f)] private float entryLifetime = 4f;
    [SerializeField, Min(1)] private int maxEntries = 5;

    private readonly Queue<TMP_Text> _entries = new();

    private void OnEnable()
    {
        MatchManager.OnKillRegistered += HandleKillRegistered;
    }

    private void OnDisable()
    {
        MatchManager.OnKillRegistered -= HandleKillRegistered;
    }

    private void HandleKillRegistered(KillEvent killEvent)
    {
        if (killFeedEntryPrefab == null || container == null)
            return;

        string victimName = killEvent.Victim != null ? killEvent.Victim.name : "Unknown";
        string message = victimName + " died";

        if (killEvent.HasKiller && killEvent.Killer != null)
        {
            string killerName = killEvent.Killer.name;
            message = killerName + "  >  " + victimName;
        }

        TMP_Text entry = Instantiate(killFeedEntryPrefab, container);
        entry.text = message;
        _entries.Enqueue(entry);

        while (_entries.Count > maxEntries)
        {
            TMP_Text oldest = _entries.Dequeue();
            if (oldest != null)
                Destroy(oldest.gameObject);
        }

        StartCoroutine(RemoveAfterLifetime(entry));
    }

    private IEnumerator RemoveAfterLifetime(TMP_Text entry)
    {
        yield return new WaitForSeconds(entryLifetime);

        if (entry != null)
        {
            RemoveEntry(entry);
            Destroy(entry.gameObject);
        }
    }

    private void RemoveEntry(TMP_Text entry)
    {
        if (_entries.Count == 0)
            return;

        TMP_Text[] snapshot = _entries.ToArray();
        _entries.Clear();
        foreach (TMP_Text item in snapshot)
        {
            if (item != null && item != entry)
                _entries.Enqueue(item);
        }
    }
}
