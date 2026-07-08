/// <summary>
/// Gun - Individual weapon/gun
/// 
/// Handles:
/// - Firing bullets
/// - Managing ammo
/// - Reload mechanics
/// - Fire rate control
/// - Spread and recoil
/// 
/// Usage:
/// Attach to Gun GameObject
/// Requires: WeaponData assigned
/// </summary>

using UnityEngine;
using System;

public class Gun : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;  // Barrel - जहाँ से bullets निकलेंगे

    // ============== PRIVATE FIELDS ==============

    private int _currentAmmo;        // Reserve ammo
    private int _ammoInClip;         // Current magazine
    private float _lastFireTime = -999f;
    private float _reloadEndTime;
    private bool _isReloading = false;

    // Events
    public static event Action<int, int> OnAmmoChanged;      // (clip, total)
    public static event Action<float> OnReloadStarted;       // (duration)
    public static event Action OnReloadCompleted;

    private void Awake()
{
    Debug.Log("Gun Awake");

    if (weaponData == null)
    {
        Debug.LogError("[Gun] WeaponData not assigned!");
        enabled = false;
        return;
    }

        InitializeAmmo();
        Debug.Log($"[Gun] ✓ Initialized: {weaponData.GetWeaponName()}");
    }

    private void OnEnable()
{
    Debug.Log("Gun OnEnable");

    InputManager.OnFireInput += HandleFireInput;
    InputManager.OnReloadInput += HandleReloadInput;

    Debug.Log("[Gun] Events subscribed");
}

    private void OnDisable()
    {
        Debug.Log("Gun OnDisable");
        // Unsubscribe
        InputManager.OnFireInput -= HandleFireInput;
        InputManager.OnReloadInput -= HandleReloadInput;
    }

    private void Update()
    {
        // Check if reload is complete
        if (_isReloading && Time.time >= _reloadEndTime)
        {
            CompleteReload();
        }
    }

    // ============== INITIALIZATION ==============

    private void InitializeAmmo()
    {
        _currentAmmo = weaponData.GetStartingAmmo();
        _ammoInClip = weaponData.GetClipSize();
        
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);

        Debug.Log($"[Gun] Ammo initialized: {_ammoInClip}/{_currentAmmo}");
    }

    // ============== FIRING ==============

    private void HandleFireInput()
    {  Debug.Log("Gun Received Fire Input");
        // Don't fire while reloading
        if (_isReloading)
            return;

        // Try to fire
        if (CanFire())
        {
            Fire();
        }
        else if (_ammoInClip <= 0 && _currentAmmo > 0)
        {
            // Auto-reload if out of ammo
            StartReload();
        }
    }

    private bool CanFire()
    {
        // Check fire rate (time between shots)
        float timeSinceLastFire = Time.time - _lastFireTime;
        if (timeSinceLastFire < (1f / weaponData.GetFireRate()))
        {
            return false;
        }

        // Check if ammo in clip
        if (_ammoInClip <= 0)
        {
            return false;
        }

        return true;
    }

    private void Fire()
{
    Debug.Log("========== FIRE START ==========");

    // Get player controller
    PlayerController player = GetComponentInParent<PlayerController>();

    Debug.Log("PlayerController = " + player);

    if (player == null)
    {
        Debug.LogError("[Gun] PlayerController NOT FOUND");
        return;
    }

    Vector2 baseAimDirection = player.GetLookDirection();

    Debug.Log("Aim Direction = " + baseAimDirection);

    for (int i = 0; i < weaponData.GetBulletsPerShot(); i++)
    {
        Debug.Log("----- Bullet " + i + " -----");

        float spreadAngle = UnityEngine.Random.Range(
            -weaponData.GetSpread(),
            weaponData.GetSpread());

        Debug.Log("Spread Angle = " + spreadAngle);

        Vector2 fireDirection =
            Quaternion.Euler(0, 0, spreadAngle * Mathf.Rad2Deg) * baseAimDirection;

        Debug.Log("Fire Direction = " + fireDirection);

        // Pool
        PoolManager pool = GameManager.Instance.GetPoolManager();

        Debug.Log("PoolManager = " + pool);

        GameObject bulletObj = pool.GetPooledObject("bullet");

        Debug.Log("Bullet Object = " + bulletObj);

        if (bulletObj == null)
        {
            Debug.LogError("Bullet Object is NULL");
            return;
        }

        Debug.Log("Bullet Name = " + bulletObj.name);

        Vector3 spawnPos =
            firePoint != null ? firePoint.position : transform.position;

        Debug.Log("Spawn Position = " + spawnPos);

        bulletObj.transform.position = spawnPos;
        bulletObj.transform.rotation = Quaternion.identity;

        Debug.Log("Bullet Position After Spawn = " + bulletObj.transform.position);

        Bullet bullet = bulletObj.GetComponent<Bullet>();

        Debug.Log("Bullet Component = " + bullet);

        if (bullet == null)
        {
            Debug.LogError("Bullet script NOT FOUND");
            return;
        }

        bullet.Initialize(
            fireDirection,
            weaponData.GetBulletSpeed(),
            weaponData.GetDamage(),
            weaponData.GetBulletLifetime(),
            weaponData.IsPenetrating(),
            weaponData.GetBulletColor(),
            weaponData.GetBulletSize()
        );

        Debug.Log("Bullet Initialize Completed");

        Rigidbody2D rb = bulletObj.GetComponent<Rigidbody2D>();

        Debug.Log("RigidBody = " + rb);

        if (rb != null)
        {
            Debug.Log("Velocity Before Active = " + rb.linearVelocity);
        }

        bulletObj.SetActive(true);

        Debug.Log("Bullet Active = " + bulletObj.activeSelf);

        if (rb != null)
        {
            Debug.Log("Velocity After Active = " + rb.linearVelocity);
            Debug.Log("Bullet Current Position = " + bulletObj.transform.position);
        }

        SpriteRenderer sr = bulletObj.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Debug.Log("Sprite Enabled = " + sr.enabled);
            Debug.Log("Sprite = " + sr.sprite);
            Debug.Log("Sorting Layer = " + sr.sortingLayerName);
            Debug.Log("Sorting Order = " + sr.sortingOrder);
            Debug.Log("Scale = " + bulletObj.transform.localScale);
        }

        Debug.Log("----- Bullet Spawn Complete -----");
    }

    _ammoInClip--;
    _lastFireTime = Time.time;

    OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);

    GameManager.Instance.GetAudioManager().PlaySFX(weaponData.GetFireSFX());

    Debug.Log("Ammo Left = " + _ammoInClip + "/" + _currentAmmo);

    Debug.Log("========== FIRE END ==========");
}

    // ============== RELOAD ==============

    private void HandleReloadInput()
    {
        // Don't reload if already reloading
        if (_isReloading)
            return;

        // Don't reload if clip is full
        if (_ammoInClip == weaponData.GetClipSize())
            return;

        // Don't reload if no ammo in reserve
        if (_currentAmmo <= 0)
            return;

        StartReload();
    }

    private void StartReload()
    {
        if (_isReloading)
            return;

        _isReloading = true;
        _reloadEndTime = Time.time + weaponData.GetReloadTime();

        // Send event
        OnReloadStarted?.Invoke(weaponData.GetReloadTime());

        // Audio feedback
        GameManager.Instance.GetAudioManager().PlaySFX(weaponData.GetReloadSFX());

        Debug.Log($"[Gun] Reloading '{weaponData.GetWeaponName()}'... ({weaponData.GetReloadTime()}s)");
    }

    private void CompleteReload()
    {
        _isReloading = false;

        // Calculate how much ammo to reload
        int ammoNeeded = weaponData.GetClipSize() - _ammoInClip;
        int ammoToTransfer = Mathf.Min(ammoNeeded, _currentAmmo);

        // Transfer ammo from reserve to clip
        _ammoInClip += ammoToTransfer;
        _currentAmmo -= ammoToTransfer;

        // Send event
        OnReloadCompleted?.Invoke();
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);

        Debug.Log($"[Gun] Reload complete! {_ammoInClip}/{_currentAmmo}");
    }

    // ============== AMMO MANAGEMENT ==============

    public void AddAmmo(int amount)
    {
        _currentAmmo = Mathf.Min(_currentAmmo + amount, weaponData.GetMaxAmmo());
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
        Debug.Log($"[Gun] Added {amount} ammo. Total: {_currentAmmo}");
    }

    public void SetAmmo(int clipAmount, int totalAmount)
    {
        _ammoInClip = Mathf.Clamp(clipAmount, 0, weaponData.GetClipSize());
        _currentAmmo = Mathf.Clamp(totalAmount, 0, weaponData.GetMaxAmmo());
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
    }

    // ============== PUBLIC GETTERS ==============

    public int GetCurrentAmmo() => _currentAmmo;
    public int GetAmmoInClip() => _ammoInClip;
    public int GetClipSize() => weaponData.GetClipSize();
    public bool IsReloading() => _isReloading;
    public WeaponData GetWeaponData() => weaponData;
    public bool IsEmpty() => _ammoInClip == 0 && _currentAmmo == 0;

    public float GetReloadProgress()
    {
        if (!_isReloading)
            return 0f;

        float elapsed = Time.time - (_reloadEndTime - weaponData.GetReloadTime());
        return elapsed / weaponData.GetReloadTime();
    }

    // ============== PUBLIC SETTERS ==============

    public void SetWeaponData(WeaponData newData)
    {
        weaponData = newData;
        InitializeAmmo();
        Debug.Log($"[Gun] Weapon changed to '{weaponData.GetWeaponName()}'");
    }

    // ============== DEBUG ==============

    public void PrintDebugInfo()
    {
        Debug.Log("=== GUN DEBUG INFO ===");
        Debug.Log($"Weapon: {weaponData.GetWeaponName()}");
        Debug.Log($"Ammo: {_ammoInClip}/{_currentAmmo}");
        Debug.Log($"Is Reloading: {_isReloading}");
        Debug.Log($"Fire Rate: {weaponData.GetFireRate()} ({1f / weaponData.GetFireRate()} shots/sec)");
        Debug.Log($"Damage: {weaponData.GetDamage()}");
        Debug.Log($"Is Empty: {IsEmpty()}");
    }
}
