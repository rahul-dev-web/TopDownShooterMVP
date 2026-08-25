/// <summary>
/// WeaponHUD - Weapon UI display
/// 
/// Ammo counter
/// Weapon name
/// Reload bar
/// </summary>

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private Image reloadBar;
    [SerializeField] private Image weaponIcon;

    private WeaponManager _weaponManager;
    private float _reloadDuration;

    private void Start()
    {
        _weaponManager = FindObjectOfType<WeaponManager>();
        if (_weaponManager == null)
        {
            Debug.LogError("[WeaponHUD] WeaponManager not found!");
            return;
        }

        // Subscribe to events
        Gun.OnAmmoChanged += UpdateAmmoDisplay;
        Gun.OnReloadStarted += StartReloadBar;
        Gun.OnReloadCompleted += CompleteReloadBar;

        UpdateDisplay();
    }

    private void Update()
    {
        if (reloadBar.fillAmount > 0 && reloadBar.fillAmount < 1)
        {
            // Update reload bar
            Gun currentGun = _weaponManager.GetCurrentGun();
            if (currentGun != null && currentGun.IsReloading())
            {
                float progress = 1f - Mathf.Clamp01(
                    _reloadDuration / currentGun.GetWeaponData().GetReloadTime()
                );
                reloadBar.fillAmount = progress;
            }
        }
    }

    private void UpdateDisplay()
    {
        Gun currentGun = _weaponManager.GetCurrentGun();
        if (currentGun == null)
            return;

        WeaponData data = currentGun.GetWeaponData();
        
        if (weaponNameText != null)
            weaponNameText.text = data.GetWeaponName();

        if (weaponIcon != null)
            weaponIcon.sprite = data.GetWeaponSprite();

        UpdateAmmoDisplay(currentGun.GetAmmoInClip(), currentGun.GetReserveAmmo());
    }

    private void UpdateAmmoDisplay(int clip, int total)
    {
        if (ammoText != null)
            ammoText.text = $"{clip} / {total}";
    }

    private void StartReloadBar(float duration)
    {
        _reloadDuration = duration;
        if (reloadBar != null)
            reloadBar.fillAmount = 1f;
    }

    private void CompleteReloadBar()
    {
        if (reloadBar != null)
            reloadBar.fillAmount = 0f;
    }

    private void OnDestroy()
    {
        Gun.OnAmmoChanged -= UpdateAmmoDisplay;
        Gun.OnReloadStarted -= StartReloadBar;
        Gun.OnReloadCompleted -= CompleteReloadBar;
    }
}