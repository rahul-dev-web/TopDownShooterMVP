/// <summary>
/// WeaponManager - Player के weapons को manage करता है
/// </summary>
using UnityEngine;
using System.Collections.Generic;
using System;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<WeaponData> startingWeapons = new List<WeaponData>();
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private bool autoSwitchOnEmpty = true;

    private readonly List<Gun> _guns = new List<Gun>();
    private int _currentWeaponIndex = -1;
    private Gun _currentGun;

    public static event Action<Gun> OnWeaponSwitched;
    public static event Action<int> OnWeaponCountChanged;

    private void Awake()
    {
        if (weaponHolder == null)
            weaponHolder = transform;

        InitializeWeapons();
        Debug.Log($"[WeaponManager] Initialized with {_guns.Count} weapons");
    }

    private void OnEnable()
    {
        InputManager.OnWeaponSwitchInput += HandleWeaponSwitch;
        Gun.OnAmmoChanged += HandleAmmoChanged;
    }

    private void OnDisable()
    {
        InputManager.OnWeaponSwitchInput -= HandleWeaponSwitch;
        Gun.OnAmmoChanged -= HandleAmmoChanged;
    }

    private void InitializeWeapons()
    {
        if (startingWeapons == null || startingWeapons.Count == 0)
        {
            Debug.LogError("[WeaponManager] No starting weapons assigned!");
            return;
        }

        GameObject gunPrefab = Resources.Load<GameObject>("Prefabs/Gun");
        if (gunPrefab == null)
        {
            Debug.LogError("[WeaponManager] Gun prefab not found at Resources/Prefabs/Gun.prefab!");
            return;
        }

        foreach (WeaponData weaponData in startingWeapons)
        {
            if (weaponData == null)
            {
                Debug.LogWarning("[WeaponManager] Null weapon data in list, skipping...");
                continue;
            }

            GameObject gunObj = Instantiate(gunPrefab, weaponHolder);
            gunObj.name = weaponData.GetWeaponName();

            Gun gun = gunObj.GetComponent<Gun>();
            if (gun == null)
            {
                Debug.LogError("[WeaponManager] Gun script not found on prefab!");
                Destroy(gunObj);
                continue;
            }

            gun.SetWeaponData(weaponData);
            gunObj.SetActive(false);
            _guns.Add(gun);
        }

        if (_guns.Count > 0)
            SwitchToWeapon(0);

        OnWeaponCountChanged?.Invoke(_guns.Count);
    }

    private void HandleWeaponSwitch()
    {
        if (_guns.Count == 0)
            return;

        int nextIndex = _currentWeaponIndex < 0 ? 0 : (_currentWeaponIndex + 1) % _guns.Count;
        SwitchToWeapon(nextIndex);
    }

    public void SwitchToWeapon(int index)
    {
        if (index < 0 || index >= _guns.Count)
        {
            Debug.LogWarning($"[WeaponManager] Invalid weapon index: {index}");
            return;
        }

        if (_currentGun != null && _currentWeaponIndex == index)
            return;

        if (_currentGun != null)
            _currentGun.gameObject.SetActive(false);

        _currentWeaponIndex = index;
        _currentGun = _guns[index];
        _currentGun.gameObject.SetActive(true);

        OnWeaponSwitched?.Invoke(_currentGun);
        Debug.Log($"[WeaponManager] Active weapon: {_currentGun.GetWeaponData().GetWeaponName()}");
    }

    public void SwitchToWeapon(string weaponName)
    {
        for (int i = 0; i < _guns.Count; i++)
        {
            WeaponData data = _guns[i].GetWeaponData();
            if (data != null && data.GetWeaponName() == weaponName)
            {
                SwitchToWeapon(i);
                return;
            }
        }

        Debug.LogWarning($"[WeaponManager] Weapon not found: {weaponName}");
    }

    private void HandleAmmoChanged(int clip, int total)
    {
        if (!autoSwitchOnEmpty || clip != 0 || total != 0 || _guns.Count <= 1)
            return;

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

    public void AddAmmoToCurrentWeapon(int amount)
    {
        if (_currentGun != null)
            _currentGun.AddAmmo(amount);
    }

    public void AddAmmoToAllWeapons(int amount)
    {
        foreach (Gun gun in _guns)
            gun.AddAmmo(amount);
    }

    public void RefillAllWeapons()
    {
        foreach (Gun gun in _guns)
        {
            WeaponData data = gun.GetWeaponData();
            if (data != null)
                gun.SetAmmo(gun.GetClipSize(), data.GetMaxAmmo());
        }
    }

    public int GetTotalAmmoAll()
    {
        int total = 0;
        foreach (Gun gun in _guns)
            total += gun.GetCurrentAmmo() + gun.GetAmmoInClip();
        return total;
    }

    public Gun GetCurrentGun() => _currentGun;
    public int GetCurrentWeaponIndex() => _currentWeaponIndex;
    public List<Gun> GetAllGuns() => _guns;
    public int GetWeaponCount() => _guns.Count;

    public Gun GetGun(int index)
    {
        return index >= 0 && index < _guns.Count ? _guns[index] : null;
    }

    public Gun GetGun(string weaponName)
    {
        foreach (Gun gun in _guns)
        {
            WeaponData data = gun.GetWeaponData();
            if (data != null && data.GetWeaponName() == weaponName)
                return gun;
        }
        return null;
    }

    public void PrintDebugInfo()
    {
        Debug.Log("=== WEAPON MANAGER DEBUG ===");
        Debug.Log($"Total Weapons: {_guns.Count}");
        Debug.Log($"Current Weapon Index: {_currentWeaponIndex}");

        if (_currentGun != null && _currentGun.GetWeaponData() != null)
            Debug.Log($"Current Weapon: {_currentGun.GetWeaponData().GetWeaponName()} | Ammo: {_currentGun.GetAmmoInClip()}/{_currentGun.GetCurrentAmmo()}");
    }

    public void PrintAllWeaponStats()
    {
        foreach (Gun gun in _guns)
        {
            WeaponData data = gun.GetWeaponData();
            if (data != null)
                data.PrintStats();
        }
    }
}