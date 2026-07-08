/// <summary>
/// WeaponData - Weapon Configuration (ScriptableObject)
/// 
/// हर weapon का configuration यहाँ define करो
/// Unity में create करके reuse कर सकते हो
/// 
/// Usage:
/// Create → Weapons → Weapon Data
/// </summary>

using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData_", menuName = "Weapons/Weapon Data")]
public class WeaponData : ScriptableObject
{
    // ============== BASIC INFO ==============

    [Header("Basic Info")]
    [SerializeField] private string weaponName = "AR-15";
    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private string weaponDescription = "Assault Rifle";

    // ============== FIRING STATS ==============

    [Header("Firing")]
    [SerializeField] private float fireRate = 0.1f;  // Time between shots (seconds)
    [SerializeField] private float damage = 10f;      // Damage per bullet
    [SerializeField] private float bulletSpeed = 20f; // Bullet travel speed
    [SerializeField] private int bulletsPerShot = 1;  // Shotgun style
    [SerializeField] private float spread = 0.1f;     // Angular spread (radians)
    [SerializeField] private float recoil = 0.5f;     // Camera shake on fire

    // ============== AMMO STATS ==============

    [Header("Ammo Management")]
    [SerializeField] private int maxAmmo = 300;        // Total ammo pool
    [SerializeField] private int clipSize = 30;        // Bullets per magazine
    [SerializeField] private int startingAmmo = 90;    // Initial ammo (3 clips)

    // ============== RELOAD ==============

    [Header("Reload")]
    [SerializeField] private float reloadTime = 2f;    // Reload duration

    // ============== BULLET PHYSICS ==============

    [Header("Bullet Physics")]
    [SerializeField] private float bulletLifetime = 5f;      // How long bullet exists
    [SerializeField] private bool penetrating = false;       // Goes through objects
    [SerializeField] private float bulletMass = 1f;          // Physics mass

    // ============== VISUALS ==============

    [Header("Visuals")]
    [SerializeField] private Color bulletColor = Color.yellow;
    [SerializeField] private float bulletSize = 0.2f;
    [SerializeField] private bool hasTracer = true;           // Visible bullet trail
    [SerializeField] private Color tracerColor = Color.yellow;

    // ============== AUDIO ==============

    [Header("Audio")]
    [SerializeField] private string fireSFX = "gunfire";
    [SerializeField] private string reloadSFX = "reload";
    [SerializeField] private float fireVolume = 0.8f;
    [SerializeField] private float reloadVolume = 0.6f;

    // ============== GETTERS ==============

    public string GetWeaponName() => weaponName;
    public Sprite GetWeaponSprite() => weaponSprite;
    public string GetDescription() => weaponDescription;

    public float GetFireRate() => fireRate;
    public float GetDamage() => damage;
    public float GetBulletSpeed() => bulletSpeed;
    public int GetBulletsPerShot() => bulletsPerShot;
    public float GetSpread() => spread;
    public float GetRecoil() => recoil;

    public int GetMaxAmmo() => maxAmmo;
    public int GetClipSize() => clipSize;
    public int GetStartingAmmo() => startingAmmo;

    public float GetReloadTime() => reloadTime;

    public float GetBulletLifetime() => bulletLifetime;
    public bool IsPenetrating() => penetrating;
    public float GetBulletMass() => bulletMass;

    public Color GetBulletColor() => bulletColor;
    public float GetBulletSize() => bulletSize;
    public bool HasTracer() => hasTracer;
    public Color GetTracerColor() => tracerColor;

    public string GetFireSFX() => fireSFX;
    public string GetReloadSFX() => reloadSFX;
    public float GetFireVolume() => fireVolume;
    public float GetReloadVolume() => reloadVolume;

    // ============== WEAPON PRESETS ==============

    public static WeaponData CreateARPreset()
    {
        WeaponData data = CreateInstance<WeaponData>();
        data.weaponName = "AR-15";
        data.fireRate = 0.1f;      // 10 shots/sec
        data.damage = 10f;
        data.bulletSpeed = 20f;
        data.clipSize = 30;
        data.reloadTime = 2f;
        data.spread = 0.15f;
        data.startingAmmo = 90;
        data.bulletColor = Color.yellow;
        return data;
    }

    public static WeaponData CreateSMGPreset()
    {
        WeaponData data = CreateInstance<WeaponData>();
        data.weaponName = "UMP-45";
        data.fireRate = 0.05f;     // 20 shots/sec
        data.damage = 7f;
        data.bulletSpeed = 15f;
        data.clipSize = 25;
        data.reloadTime = 1.5f;
        data.spread = 0.25f;
        data.startingAmmo = 75;
        data.bulletColor = Color.cyan;
        return data;
    }

    public static WeaponData CreateSniperPreset()
    {
        WeaponData data = CreateInstance<WeaponData>();
        data.weaponName = "AWP-Dragon Lore";
        data.fireRate = 0.5f;      // 2 shots/sec
        data.damage = 50f;
        data.bulletSpeed = 30f;
        data.clipSize = 5;
        data.reloadTime = 3f;
        data.spread = 0.05f;
        data.startingAmmo = 25;
        data.bulletColor = Color.green;
        return data;
    }

    public static WeaponData CreateExplosivePreset()
    {
        WeaponData data = CreateInstance<WeaponData>();
        data.weaponName = "Rocket Launcher";
        data.fireRate = 0.3f;      // 3 shots/sec
        data.damage = 30f;
        data.bulletSpeed = 18f;
        data.clipSize = 10;
        data.reloadTime = 2.5f;
        data.spread = 0.2f;
        data.startingAmmo = 30;
        data.bulletColor = Color.red;
        return data;
    }

    // ============== DEBUG ==============

    public void PrintStats()
    {
        Debug.Log("=== WEAPON STATS ===");
        Debug.Log($"Name: {weaponName}");
        Debug.Log($"Damage: {damage}");
        Debug.Log($"Fire Rate: {fireRate} (${1f / fireRate} shots/sec)");
        Debug.Log($"Clip Size: {clipSize}");
        Debug.Log($"Reload Time: {reloadTime}s");
        Debug.Log($"Spread: {spread}");
    }
}
