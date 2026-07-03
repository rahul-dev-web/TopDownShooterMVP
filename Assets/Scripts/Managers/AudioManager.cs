/// <summary>
/// AudioManager - Audio System Manager
/// सभी audio (music, SFX, ambience) को manage करता है
/// 
/// Usage:
/// GameManager.Instance.GetAudioManager().PlaySFX("gunfire");
/// GameManager.Instance.GetAudioManager().PlayMusic("menu_theme");
/// </summary>

using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    // Audio sources
    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private AudioSource _ambienceSource;

    // Audio clips dictionary
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    // Volume control
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private float ambienceVolume = 0.5f;

    private float _masterVolume = 1f;

    // Audio settings
    [SerializeField] private bool loadClipsFromResources = true;

    private void OnEnable()
    {
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("[AudioManager] Initializing...");

        // Create audio sources
        _musicSource = CreateAudioSource("MusicSource", musicVolume);
        _sfxSource = CreateAudioSource("SFXSource", sfxVolume);
        _ambienceSource = CreateAudioSource("AmbienceSource", ambienceVolume);

        // Set audio source settings
        _musicSource.loop = true;
        _sfxSource.loop = false;
        _ambienceSource.loop = true;

        // Load audio clips from Resources folder
        if (loadClipsFromResources)
        {
            LoadAudioClips();
        }

        Debug.Log("[AudioManager] ✓ Initialized");
    }

    /// <summary>
    /// AudioSource create करता है child object के रूप में
    /// </summary>
    private AudioSource CreateAudioSource(string name, float volume)
    {
        GameObject audioSourceObj = new GameObject(name);
        audioSourceObj.transform.SetParent(transform);
        audioSourceObj.transform.localPosition = Vector3.zero;

        AudioSource audioSource = audioSourceObj.AddComponent<AudioSource>();
        audioSource.volume = volume * _masterVolume;

        return audioSource;
    }

    /// <summary>
    /// Resources/Audio folder से सभी clips load करता है
    /// </summary>
    private void LoadAudioClips()
    {
        // Music clips
        LoadClipsFromFolder("Audio/Music", "music_");

        // SFX clips
        LoadClipsFromFolder("Audio/SFX", "sfx_");

        // Ambience clips
        LoadClipsFromFolder("Audio/Ambience", "amb_");

        Debug.Log($"[AudioManager] Loaded {_audioClips.Count} audio clips");
    }

    /// <summary>
    /// एक folder से सभी audio clips load करता है
    /// </summary>
    private void LoadClipsFromFolder(string folderPath, string prefix)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(folderPath);

        foreach (AudioClip clip in clips)
        {
            string clipName = prefix + clip.name.ToLower();
            _audioClips[clipName] = clip;
        }
    }

    // ============== SFX METHODS ==============

    public void PlaySFX(string sfxName)
    {
        if (!_audioClips.ContainsKey("sfx_" + sfxName.ToLower()))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {sfxName}");
            return;
        }

        AudioClip clip = _audioClips["sfx_" + sfxName.ToLower()];
        _sfxSource.PlayOneShot(clip, sfxVolume * _masterVolume);
    }

    public void PlaySFXAtPosition(string sfxName, Vector3 position)
    {
        if (!_audioClips.ContainsKey("sfx_" + sfxName.ToLower()))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: {sfxName}");
            return;
        }

        AudioClip clip = _audioClips["sfx_" + sfxName.ToLower()];
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * _masterVolume);
    }

    public void StopSFX()
    {
        _sfxSource.Stop();
    }

    // ============== MUSIC METHODS ==============

    public void PlayMusic(string musicName, bool loop = true)
    {
        string key = "music_" + musicName.ToLower();

        if (!_audioClips.ContainsKey(key))
        {
            Debug.LogWarning($"[AudioManager] Music not found: {musicName}");
            return;
        }

        // अगर पहले से same music play हो रहा है, तो stop करो नहीं
        if (_musicSource.clip != null && _musicSource.clip.name == musicName)
        {
            return;
        }

        AudioClip clip = _audioClips[key];
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.Play();

        Debug.Log($"[AudioManager] Playing music: {musicName}");
    }

    public void StopMusic(float fadeOutDuration = 0.5f)
    {
        if (fadeOutDuration <= 0)
        {
            _musicSource.Stop();
        }
        else
        {
            StartCoroutine(FadeOutMusic(fadeOutDuration));
        }
    }

    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    public void ResumeMusic()
    {
        _musicSource.Play();
    }

    private System.Collections.IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = _musicSource.volume;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / duration);
            yield return null;
        }

        _musicSource.Stop();
        _musicSource.volume = startVolume;
    }

    // ============== AMBIENCE METHODS ==============

    public void PlayAmbience(string ambienceName, bool loop = true)
    {
        string key = "amb_" + ambienceName.ToLower();

        if (!_audioClips.ContainsKey(key))
        {
            Debug.LogWarning($"[AudioManager] Ambience not found: {ambienceName}");
            return;
        }

        AudioClip clip = _audioClips[key];
        _ambienceSource.clip = clip;
        _ambienceSource.loop = loop;
        _ambienceSource.Play();

        Debug.Log($"[AudioManager] Playing ambience: {ambienceName}");
    }

    public void StopAmbience()
    {
        _ambienceSource.Stop();
    }

    // ============== VOLUME CONTROL ==============

    public void SetMasterVolume(float volume)
    {
        _masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    public void SetAmbienceVolume(float volume)
    {
        ambienceVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }

    private void UpdateAllVolumes()
    {
        _musicSource.volume = musicVolume * _masterVolume;
        _sfxSource.volume = sfxVolume * _masterVolume;
        _ambienceSource.volume = ambienceVolume * _masterVolume;
    }

    // ============== GETTERS ==============

    public float GetMasterVolume() => _masterVolume;
    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
    public float GetAmbienceVolume() => ambienceVolume;

    public bool IsMusicPlaying() => _musicSource.isPlaying;
    public bool IsSFXPlaying() => _sfxSource.isPlaying;

    // ============== DEBUG ==============

    public void PrintLoadedClips()
    {
        Debug.Log("=== LOADED AUDIO CLIPS ===");
        foreach (var clip in _audioClips)
        {
            Debug.Log($"  {clip.Key}: {clip.Value.length}s");
        }
    }
}