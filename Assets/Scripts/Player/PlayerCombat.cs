using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int playerDamage = 20;

    [Header("Weapon Hitbox")]
    [Tooltip("Gắn WeaponHitbox nằm trên vũ khí (object có BoxCollider IsTrigger)")]
    [SerializeField] private WeaponHitbox weaponHitbox;

    [Header("SFX")]
    [SerializeField] private string slashSoundName = "se_slash";
    [Range(0f, 1f)]
    [SerializeField] private float slashVolume = 1f;

    public static PlayerCombat Instance { get; private set; }

    private int basePlayerDamage;

    private void Awake()
    {
        Instance = this;
        basePlayerDamage = playerDamage;

        if (weaponHitbox != null)
        {
            weaponHitbox.SetDamage(playerDamage);
            weaponHitbox.DisableHitbox();
        }
    }

    private void Start()
    {
        ApplyBonusStats();
    }

    public void ApplyBonusStats()
    {
        int bonus = 0;
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            bonus = PlayerDataManager.Instance.playerData.bonusDamage;

        playerDamage = basePlayerDamage + bonus;
        if (weaponHitbox != null)
            weaponHitbox.SetDamage(playerDamage);
    }

    public void AddDamage(int amount)
    {
        playerDamage += amount;
        if (weaponHitbox != null)
            weaponHitbox.SetDamage(playerDamage);
    }

    public int GetPlayerDamage() => playerDamage;

    private void OnValidate()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.SetDamage(playerDamage);
        }
    }

    // ===== Animation Events (gắn vào Attack clip) =====
    public void AnimationEvent_EnableWeaponHitbox()
    {
        if (weaponHitbox == null) return;
        weaponHitbox.SetDamage(playerDamage);
        weaponHitbox.EnableHitbox();

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(slashSoundName))
        {
            AudioManager.Instance.PlaySound(slashSoundName, slashVolume);
        }
    }

    public void AnimationEvent_DisableWeaponHitbox()
    {
        if (weaponHitbox == null) return;
        weaponHitbox.DisableHitbox();
    }
}

