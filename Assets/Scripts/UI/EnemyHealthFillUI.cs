using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthFillUI : MonoBehaviour
{
    [Tooltip("Image Fill (Image Type = Filled)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Nếu để trống sẽ tự tìm EnemyHealth ở parent")]
    [SerializeField] private EnemyHealth enemyHealth;

    private bool isSubscribed = false;

    private void Awake()
    {
        if (fillImage == null)
        {
            fillImage = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        TryBindAndSubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        if (enemyHealth == null || !isSubscribed)
        {
            TryBindAndSubscribe();
        }

        if (enemyHealth != null)
        {
            UpdateFill(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
        }
    }

    private void TryBindAndSubscribe()
    {
        if (enemyHealth == null)
        {
            enemyHealth = GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null) return;

        if (!isSubscribed)
        {
            enemyHealth.OnHealthChanged += HandleHealthChanged;
            isSubscribed = true;
        }

        UpdateFill(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
    }

    private void Unsubscribe()
    {
        if (enemyHealth == null || !isSubscribed) return;
        enemyHealth.OnHealthChanged -= HandleHealthChanged;
        isSubscribed = false;
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateFill(current, max);
    }

    private void UpdateFill(int current, int max)
    {
        if (fillImage == null) return;
        float ratio = max <= 0 ? 0f : (float)current / max;
        fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}

