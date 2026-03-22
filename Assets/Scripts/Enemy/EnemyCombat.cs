using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int enemyDamage = 10;

    [Header("Weapon Hitbox")]
    [Tooltip("Gắn EnemyWeaponHitbox nằm trên vũ khí enemy (object có Collider IsTrigger)")]
    [SerializeField] private EnemyWeaponHitbox weaponHitbox;

    [Header("SFX")]
    [SerializeField] private string attackSoundName = "se_slash";
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 1f;

    private void Awake()
    {
        if (weaponHitbox != null)
        {
            weaponHitbox.SetDamage(enemyDamage);
            weaponHitbox.DisableHitbox();
        }
    }

    public void SetDamage(int damage)
    {
        enemyDamage = damage;
        if (weaponHitbox != null)
            weaponHitbox.SetDamage(enemyDamage);
    }

    // ===== Animation Events (gắn vào Attack clip của enemy) =====
    public void AnimationEvent_EnableWeaponHitbox()
    {
        if (weaponHitbox == null) return;
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && (health.IsDead || health.CurrentHealth <= 0))
            return;

        weaponHitbox.SetDamage(enemyDamage);
        weaponHitbox.EnableHitbox();

        if (AudioManager.Instance != null && !string.IsNullOrEmpty(attackSoundName))
        {
            AudioManager.Instance.PlaySound(attackSoundName, attackVolume);
        }
    }

    public void AnimationEvent_DisableWeaponHitbox()
    {
        if (weaponHitbox == null) return;
        weaponHitbox.DisableHitbox();
    }
}
