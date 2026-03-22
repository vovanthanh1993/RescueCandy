using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthFillUI : MonoBehaviour
{
    [Tooltip("Image Fill (Image Type = Filled)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Tốc độ fill giảm/tăng mượt (càng lớn càng nhanh)")]
    [SerializeField] private float fillSpeed = 5f;

    [Tooltip("Nếu để trống sẽ tự tìm EnemyHealth ở parent")]
    [SerializeField] private EnemyHealth enemyHealth;

    private bool isSubscribed = false;
    private float targetFillAmount = 1f;

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
        targetFillAmount = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);

        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFillAmount, fillSpeed * Time.deltaTime);
        }
    }
}
