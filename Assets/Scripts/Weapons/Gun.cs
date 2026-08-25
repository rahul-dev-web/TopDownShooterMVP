/// <summary>
/// Gun - Individual weapon/gun.
/// Handles firing, ammo, reload, fire rate, spread and projectile ownership.
/// </summary>
using System;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private WeaponData weaponData;
    [SerializeField] private Transform firePoint;

    private int _currentAmmo;
    private int _ammoInClip;
    private float _lastFireTime = -999f;
    private float _reloadEndTime;
    private bool _isReloading;
    private GameObject _owner;

    public static event Action<int, int> OnAmmoChanged;
    public static event Action<float> OnReloadStarted;
    public static event Action OnReloadCompleted;
    public static event Action<Gun, GameObject> OnShotFired;

    private void Awake()
    {
        _owner = ResolveOwner();

        // WeaponManager can inject WeaponData immediately after instantiation.
        // Do not disable the component when the prefab has no default data.
        if (weaponData != null)
            InitializeAmmo();
    }

    private void OnEnable()
    {
        InputManager.OnFireInput += HandleFireInput;
        InputManager.OnReloadInput += HandleReloadInput;
    }

    private void OnDisable()
    {
        InputManager.OnFireInput -= HandleFireInput;
        InputManager.OnReloadInput -= HandleReloadInput;
    }

    private void Update()
    {
        if (_isReloading && Time.time >= _reloadEndTime)
            CompleteReload();
    }

    private void InitializeAmmo()
    {
        if (weaponData == null)
            return;

        _currentAmmo = weaponData.GetStartingAmmo();
        _ammoInClip = weaponData.GetClipSize();
        _isReloading = false;
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
    }

    public void SetWeaponData(WeaponData data)
    {
        if (data == null)
        {
            Debug.LogError("[Gun] Cannot assign null WeaponData.");
            return;
        }

        weaponData = data;
        _owner ??= ResolveOwner();
        InitializeAmmo();
    }

    private void HandleFireInput()
    {
        if (weaponData == null || _isReloading)
            return;

        if (CanFire())
            Fire();
        else if (_ammoInClip <= 0 && _currentAmmo > 0)
            StartReload();
    }

    private void HandleReloadInput()
    {
        StartReload();
    }

    private bool CanFire()
    {
        if (weaponData == null || weaponData.GetFireRate() <= 0f)
            return false;

        if (Time.time - _lastFireTime < 1f / weaponData.GetFireRate())
            return false;

        return _ammoInClip > 0;
    }

    private void Fire()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("[Gun] PlayerController not found in parent hierarchy.");
            return;
        }

        _owner ??= ResolveOwner();
        Vector2 baseAimDirection = player.GetLookDirection();
        if (baseAimDirection.sqrMagnitude <= 0.0001f)
            return;

        PoolManager pool = GameManager.Instance?.GetPoolManager();
        if (pool == null)
        {
            Debug.LogError("[Gun] PoolManager unavailable.");
            return;
        }

        int spawnedCount = 0;
        for (int i = 0; i < weaponData.GetBulletsPerShot(); i++)
        {
            float spreadAngle = UnityEngine.Random.Range(-weaponData.GetSpread(), weaponData.GetSpread());
            Vector2 fireDirection = Quaternion.Euler(0f, 0f, spreadAngle * Mathf.Rad2Deg) * baseAimDirection;

            GameObject bulletObj = pool.GetPooledObject("bullet");
            if (bulletObj == null)
            {
                Debug.LogWarning("[Gun] Bullet pool returned no object.");
                continue;
            }

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
            bulletObj.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

            Bullet bullet = bulletObj.GetComponent<Bullet>();
            if (bullet == null)
            {
                Debug.LogError("[Gun] Bullet component missing on pooled object.");
                pool.ReturnPooledObject("bullet", bulletObj);
                continue;
            }

            bullet.Initialize(
                fireDirection,
                weaponData.GetBulletSpeed(),
                weaponData.GetDamage(),
                weaponData.GetBulletLifetime(),
                weaponData.IsPenetrating(),
                weaponData.GetBulletColor(),
                weaponData.GetBulletSize(),
                _owner);

            bulletObj.SetActive(true);
            spawnedCount++;
        }

        if (spawnedCount <= 0)
            return;

        _ammoInClip--;
        _lastFireTime = Time.time;
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
        OnShotFired?.Invoke(this, _owner);

        GameManager.Instance?.GetAudioManager()?.PlaySFX(weaponData.GetFireSFX());
    }

    private void StartReload()
    {
        if (_isReloading || weaponData == null)
            return;
        if (_ammoInClip >= weaponData.GetClipSize() || _currentAmmo <= 0)
            return;

        _isReloading = true;
        _reloadEndTime = Time.time + Mathf.Max(0f, weaponData.GetReloadTime());
        OnReloadStarted?.Invoke(Mathf.Max(0f, weaponData.GetReloadTime()));
    }

    private void CompleteReload()
    {
        int needed = weaponData.GetClipSize() - _ammoInClip;
        int loaded = Mathf.Min(needed, _currentAmmo);
        _ammoInClip += loaded;
        _currentAmmo -= loaded;
        _isReloading = false;

        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
        OnReloadCompleted?.Invoke();
    }

    private GameObject ResolveOwner()
    {
        PlayerController player = GetComponentInParent<PlayerController>();
        if (player != null)
            return player.gameObject;

        Transform root = transform.root;
        return root != null ? root.gameObject : gameObject;
    }

    public void SetOwner(GameObject owner) => _owner = owner;
    public GameObject GetOwner() => _owner;
    public WeaponData GetWeaponData() => weaponData;
    public int GetAmmoInClip() => _ammoInClip;
    public int GetReserveAmmo() => _currentAmmo;
    public int GetCurrentAmmo() => _currentAmmo;
    public bool IsReloading() => _isReloading;
    public int GetClipSize() => weaponData != null ? weaponData.GetClipSize() : 0;

    public void AddAmmo(int amount)
    {
        if (amount <= 0)
            return;

        int maxAmmo = weaponData != null ? weaponData.GetMaxAmmo() : int.MaxValue;
        _currentAmmo = Mathf.Clamp(_currentAmmo + amount, 0, maxAmmo);
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
    }

    public void SetAmmo(int clipAmmo, int reserveAmmo)
    {
        if (weaponData == null)
            return;

        _ammoInClip = Mathf.Clamp(clipAmmo, 0, weaponData.GetClipSize());
        _currentAmmo = Mathf.Clamp(reserveAmmo, 0, weaponData.GetMaxAmmo());
        _isReloading = false;
        OnAmmoChanged?.Invoke(_ammoInClip, _currentAmmo);
    }
}
