/// <summary>
/// WeaponManager - Player के weapons को manage करता है
/// 
/// Multiple weapons support
/// Weapon switching
/// Gun attachment/detachment
/// 
/// Usage:
/// Attach to Player GameObject
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using System;

public class WeaponManager : MonoBehaviour
{
    // ============== SERIALIZED FIELDS ==============

    [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();
    [SerializeField] private Transform weaponHolder;  // Parent object for weapons
    [SerializeField] private bool autoSwitchOnEmpty = true;

    // ============== PRIVATE FIELDS ==============

    private List<Gun> _guns = new List<Gun>();
    private int _currentWeaponIndex = 0;
    private Gun _currentGun;

    // Events
    public static event Action<Gun> OnWeaponSwitched;
    public static event Action<int> OnWeaponCountChanged;

    private void Awake()
    {
        // Setup weapon holder if not assigned
        if (weaponHolder == null)
        {
            weaponHolder = transform;
        }

        InitializeWeapons();
        Debug.Log($"[WeaponManager] ✓ Initialized with {_guns.Count} weapons");
    }

    private void OnEnable()
    {
        // Subscribe to input events
        InputManager.OnWeaponSwitchInput += HandleWeaponSwitch;

        // Subscribe to gun events for auto-reload
        Gun.OnAmmoChanged += HandleAmmoChanged;

        Debug.Log("[WeaponManager] Events subscribed");
    }

    private void OnDisable()
    {
        // Unsubscribe
        InputManager.OnWeaponSwitchInput -= HandleWeaponSwitch;
        Gun.OnAmmoChanged -= HandleAmmoChanged;
    }

    // ============== INITIALIZATION ==============

    private void InitializeWeapons()
    {
        if (startingWeapons.Count == 0)
        {
            Debug.LogError("[WeaponManager] No starting weapons assigned!");
            return;
        }

        // Load gun prefab from resources
        GameObject gunPrefab = Resources.Load<GameObject>("Prefabs/Gun");
        if (gunPrefab == null)
        {
            Debug.LogError("[WeaponManager] Gun prefab not found at Resources/Prefabs/Gun.prefab!");
            return;
        }

        // Create gun instance for each weapon data
        foreach (WeaponData weaponData in startingWeapons)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("[WeaponManager] Null weapon data in list, skipping...");
                continue;
            }

            // Instantiate gun
            GameObject gunObj = Instantiate(gunPrefab, weaponHolder);
            gunObj.name = weaponData.GetWeaponName();

            // Get Gun component
            Gun gun = gunObj.GetComponent<Gun>();
            if (gun == null)
            {
                Debug.LogError($"[WeaponManager] Gun script not found on prefab!");
                continue;
            }

            // Setup gun with weapon data
            gun.SetWeaponData(weaponData);
            _guns.Add(gun);

            // Deactivate initially (will activate first one)
            gunObj.SetActive(false);

            Debug.Log($"[WeaponManager] Added weapon: {weaponData.GetWeaponName()}");
        }

        // Activate first weapon
        if (_guns.Count > 0)
        {
            SwitchToWeapon(0);
        }

        OnWeaponCountChanged?.Invoke(_guns.Count);
    }

    // ============== WEAPON SWITCHING ==============

    private void HandleWeaponSwitch()
    {
        // Cycle to next weapon
        int nextIndex = (_currentWeaponIndex + 1) % _guns.Count;
        SwitchToWeapon(nextIndex);
    }

    public void SwitchToWeapon(int index)
    {
        // Validate index
        if (index < 0 || index >= _guns.Count)
        {
            Debug.LogWarning($"[WeaponManager] Invalid weapon index: {index}");
            return;
        }

        // Don't switch if already on this weapon
        if (_currentWeaponIndex == index)
            return;

        // Deactivate current gun
        if (_currentGun != null)
        {
            _currentGun.gameObject.SetActive(false);
        }

        // Activate new gun
        _currentWeaponIndex = index;
        _currentGun = _guns[index];
        _currentGun.gameObject.SetActive(true);

        // Send event
        OnWeaponSwitched?.Invoke(_currentGun);

        Debug.Log($"[WeaponManager] Switched to weapon {index}: {_currentGun.GetWeaponData().GetWeaponName()}");
    }

    public void SwitchToWeapon(string weaponName)
    {
        // Find weapon by name
        for (int i = 0; i < _guns.Count; i++)
        {
            if (_guns[i].GetWeaponData().GetWeaponName() == weaponName)
            {
                SwitchToWeapon(i);
                return;
            }
        }

        Debug.LogWarning($"[WeaponManager] Weapon not found: {weaponName}");
    }

    private void HandleAmmoChanged(int clip, int total)
    {
        // Auto-switch if gun is empty
        if (autoSwitchOnEmpty && clip == 0 && total == 0)
        {
            // Switch to next gun that has ammo
            for (int i = 1; i < _guns.Count; i++)
            {
                int nextIndex = (_currentWeaponIndex + i) % _guns.Count;
                Gun nextGun = _guns[nextIndex];

                if (nextGun.GetCurrentAmmo() > 0 || nextGun.GetAmmoInClip() > 0)
                {
                    SwitchToWeapon(nextIndex);
                    return;
                }
            }
        }
    }

    // ============== AMMUNITION MANAGEMENT ==============

    public void AddAmmoToCurrentWeapon(int amount)
    {
        if (_currentGun != null)
        {
            _currentGun.AddAmmo(amount);
        }
    }

    public void AddAmmoToAllWeapons(int amount)
    {
        foreach (Gun gun in _guns)
        {
            gun.AddAmmo(amount);
        }
    }

    public void RefillAllWeapons()
    {
        foreach (Gun gun in _guns)
        {
            gun.SetAmmo(gun.GetClipSize(), gun.GetWeaponData().GetMaxAmmo());
        }
    }

    public int GetTotalAmmoAll()
    {
        int total = 0;
        foreach (Gun gun in _guns)
        {
            total += gun.GetCurrentAmmo() + gun.GetAmmoInClip();
        }
        return total;
    }

    // ============== PUBLIC GETTERS ==============

    public Gun GetCurrentGun() => _currentGun;
    public int GetCurrentWeaponIndex() => _currentWeaponIndex;
    public List<Gun> GetAllGuns() => _guns;
    public int GetWeaponCount() => _guns.Count;

    public Gun GetGun(int index)
    {
        if (index >= 0 && index < _guns.Count)
            return _guns[index];
        return null;
    }

    public Gun GetGun(string weaponName)
    {
        foreach (Gun gun in _guns)
        {
            if (gun.GetWeaponData().GetWeaponName() == weaponName)
                return gun;
        }
        return null;
    }

    // ============== DEBUG ==============

    public void PrintDebugInfo()
    {
        Debug.Log("=== WEAPON MANAGER DEBUG ===");
        Debug.Log($"Total Weapons: {_guns.Count}");
        Debug.Log($"Current Weapon Index: {_currentWeaponIndex}");

        if (_currentGun != null)
        {
            Debug.Log($"Current Weapon: {_currentGun.GetWeaponData().GetWeaponName()}");
            Debug.Log($"Current Ammo: {_currentGun.GetAmmoInClip()}/{_currentGun.GetCurrentAmmo()}");
            Debug.Log($"Is Reloading: {_currentGun.IsReloading()}");
        }

        Debug.Log("All Weapons:");
        for (int i = 0; i < _guns.Count; i++)
        {
            Gun gun = _guns[i];
            Debug.Log($"  [{i}] {gun.GetWeaponData().GetWeaponName()} - {gun.GetAmmoInClip()}/{gun.GetCurrentAmmo()}");
        }
    }

    public void PrintAllWeaponStats()
    {
        Debug.Log("=== ALL WEAPON STATS ===");
        foreach (Gun gun in _guns)
        {
            gun.GetWeaponData().PrintStats();
        }
    }
}
