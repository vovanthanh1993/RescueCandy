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

    private void Start()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        maxHealth = Mathf.Clamp(maxHealth, 1, int.MaxValue);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth == 0) currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0) return;
        if (currentHealth <= 0) return;

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
