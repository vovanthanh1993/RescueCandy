using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManaFillUI : MonoBehaviour
{
    [Tooltip("Image Fill (Image Type = Filled)")]
    [SerializeField] private Image fillImage;

    [Tooltip("Text hiển thị current/max Mana")]
    [SerializeField] private TextMeshProUGUI manaText;

    [Tooltip("Tốc độ fill chuyển động mượt (càng lớn càng nhanh)")]
    [SerializeField] private float fillSpeed = 5f;

    private PlayerMana playerMana;
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
        if (playerMana == null || !isSubscribed)
        {
            TryBindAndSubscribe();
        }

        if (playerMana != null)
        {
            UpdateFill(playerMana.CurrentMana, playerMana.MaxMana);
        }
    }

    private void TryBindAndSubscribe()
    {
        if (playerMana == null)
        {
            playerMana = PlayerMana.Instance;
        }

        if (playerMana == null)
        {
            playerMana = Object.FindFirstObjectByType<PlayerMana>();
        }

        if (playerMana == null) return;

        if (!isSubscribed)
        {
            playerMana.OnManaChanged += HandleManaChanged;
            isSubscribed = true;
        }

        UpdateFill(playerMana.CurrentMana, playerMana.MaxMana);
    }

    private void Unsubscribe()
    {
        if (playerMana == null || !isSubscribed) return;
        playerMana.OnManaChanged -= HandleManaChanged;
        isSubscribed = false;
    }

    private void HandleManaChanged(int current, int max)
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

        if (manaText != null)
        {
            manaText.text = $"{current}/{max}";
        }
    }
}
