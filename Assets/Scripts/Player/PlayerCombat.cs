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

    private void Awake()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.SetDamage(playerDamage);
            weaponHitbox.DisableHitbox();
        }
    }

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

