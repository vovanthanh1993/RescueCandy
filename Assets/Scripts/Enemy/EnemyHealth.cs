using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 50;
    [SerializeField] private int currentHealth = 50;
    [SerializeField] private GameObject healthBarRoot;

    [Header("Die Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string dieTrigger = "Die";
    [SerializeField] private bool disableCollidersOnDie = true;
    [SerializeField] private float destroyDelayAfterDie = 2f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public System.Action<int, int> OnHealthChanged; // (current, max)

    private bool isDead = false;
    private bool isDestroyed = false;
    private EnemyHitFlash hitFlash;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        hitFlash = GetComponentInChildren<EnemyHitFlash>();

        if (healthBarRoot == null)
        {
            Canvas canvas = GetComponentInChildren<Canvas>();
            if (canvas != null)
            {
                healthBarRoot = canvas.gameObject;
            }
        }

        maxHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (currentHealth <= 0 || isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        hitFlash?.Flash();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // Ẩn thanh máu ngay khi chết
        if (healthBarRoot != null)
        {
            healthBarRoot.SetActive(false);
        }

        // Tắt AI nếu có
        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        // Tắt vũ khí — không gây sát thương player sau khi chết
        EnemyWeaponHitbox[] weaponHitboxes = GetComponentsInChildren<EnemyWeaponHitbox>(true);
        for (int i = 0; i < weaponHitboxes.Length; i++)
        {
            if (weaponHitboxes[i] != null)
                weaponHitboxes[i].DisableHitbox();
        }

        // Xóa toàn bộ Rigidbody (cả con) để tránh physics tác động sau khi chết
        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < rbs.Length; i++)
        {
            Destroy(rbs[i]);
        }

        // Tắt collider để không bị hit/va chạm thêm
        if (disableCollidersOnDie)
        {
            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = false;
            }
        }

        // Trigger animation chết
        if (animator != null && !string.IsNullOrEmpty(dieTrigger))
        {
            animator.SetTrigger(dieTrigger);
        }

        // Tự destroy sau một khoảng thời gian (không dùng Animation Event)
        CancelInvoke(nameof(DestroyEnemyNow));
        Invoke(nameof(DestroyEnemyNow), Mathf.Max(0f, destroyDelayAfterDie));
    }

    public void DestroyEnemyNow()
    {
        if (isDestroyed) return;
        isDestroyed = true;
        Destroy(gameObject);
    }
}

