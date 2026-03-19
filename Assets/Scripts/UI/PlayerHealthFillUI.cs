using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthFillUI : MonoBehaviour
{
    [Tooltip("Image Fill (Image Type = Filled)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Text hiển thị current/max HP")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Tooltip("Nếu để trống sẽ dùng PlayerHealth.Instance")]
    [SerializeField] private PlayerHealth playerHealth;

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
        // Nếu PlayerHealth được tạo muộn, tự bind lại
        if (playerHealth == null || !isSubscribed)
        {
            TryBindAndSubscribe();
        }

        // Fail-safe: luôn cập nhật fill theo HP hiện tại
        if (playerHealth != null)
        {
            UpdateFill(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void TryBindAndSubscribe()
    {
        if (playerHealth == null)
        {
            playerHealth = PlayerHealth.Instance;
        }

        // Fallback: tìm trong scene nếu singleton chưa set (thứ tự Awake)
        if (playerHealth == null)
        {
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        }

        if (playerHealth == null) return;

        if (!isSubscribed)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            isSubscribed = true;
        }

        UpdateFill(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void Unsubscribe()
    {
        if (playerHealth == null || !isSubscribed) return;
        playerHealth.OnHealthChanged -= HandleHealthChanged;
        isSubscribed = false;
    }

    private void HandleHealthChanged(int current, int max)
    {
        UpdateFill(current, max);
    }

    private void UpdateFill(int current, int max)
    {
        if (fillImage != null)
        {
            float ratio = max <= 0 ? 0f : (float)current / max;
            fillImage.fillAmount = Mathf.Clamp01(ratio);
        }

        if (healthText != null)
        {
            healthText.text = $"{current}/{max}";
        }
    }
}

