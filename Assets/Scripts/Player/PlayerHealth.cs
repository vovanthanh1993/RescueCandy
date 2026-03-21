using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private float hurtSoundCooldown = 0.15f;

    private PlayerHitFlash hitFlash;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public System.Action<int, int> OnHealthChanged; // (current, max)

    private float lastHurtSoundTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        hitFlash = GetComponentInChildren<PlayerHitFlash>();
    }

    private int baseMaxHealth;

    private void Start()
    {
        baseMaxHealth = maxHealth;
        ApplyBonusStats();
        ResetHealth();
    }

    public void ApplyBonusStats()
    {
        int bonus = 0;
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            bonus = PlayerDataManager.Instance.playerData.bonusHealth;

        maxHealth = baseMaxHealth + bonus;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddMaxHealth(int amount)
    {
        maxHealth += amount;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        maxHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private bool isShielded = false;

    public bool IsShielded => isShielded;

    public void SetShielded(bool shielded)
    {
        isShielded = shielded;
    }

    public void TakeDamageIgnoreShield(int damage)
    {
        if (damage <= 0) return;
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        hitFlash?.Flash();

        if (AudioManager.Instance != null && Time.time - lastHurtSoundTime >= hurtSoundCooldown)
        {
            lastHurtSoundTime = Time.time;
            AudioManager.Instance.PlayHurtSound();
        }

        if (currentHealth <= 0)
        {
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.DieFromHPZero();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (currentHealth <= 0) return;
        if (isShielded) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Hiệu ứng chớp đỏ khi bị đánh trúng
        hitFlash?.Flash();

        // SFX: player hurt (chống spam nếu dính nhiều hit liên tục)
        if (AudioManager.Instance != null && Time.time - lastHurtSoundTime >= hurtSoundCooldown)
        {
            lastHurtSoundTime = Time.time;
            AudioManager.Instance.PlayHurtSound();
        }

        if (currentHealth <= 0)
        {
            if (PlayerController.Instance != null)
            {
                // Không còn "3 mạng": hết máu thì thua game
                PlayerController.Instance.DieFromHPZero();
            }
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        maxHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        int newHealth = Mathf.Min(maxHealth, currentHealth + amount);
        if (newHealth == currentHealth) return;

        currentHealth = newHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}
