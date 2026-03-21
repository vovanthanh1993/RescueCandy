using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldSkillButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button skillButton;
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Settings")]
    [SerializeField] private bool showCooldownTimer = true;

    private bool isInitialized = false;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (isInitialized) return;

        if (skillButton == null)
            skillButton = GetComponent<Button>();

        if (skillButton != null)
        {
            skillButton.onClick.RemoveAllListeners();
            skillButton.onClick.AddListener(OnSkillButtonClicked);
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnShieldCooldownChanged += UpdateCooldownUI;
            PlayerController.Instance.OnShieldStateChanged += UpdateSkillStateUI;

            UpdateCooldownUI(PlayerController.Instance.GetShieldCooldownProgress());
            UpdateSkillStateUI(PlayerController.Instance.IsShieldActive());
        }

        isInitialized = true;
    }

    private void OnEnable()
    {
        if (isInitialized && PlayerController.Instance != null)
        {
            PlayerController.Instance.OnShieldCooldownChanged += UpdateCooldownUI;
            PlayerController.Instance.OnShieldStateChanged += UpdateSkillStateUI;
        }
    }

    private void OnDisable()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnShieldCooldownChanged -= UpdateCooldownUI;
            PlayerController.Instance.OnShieldStateChanged -= UpdateSkillStateUI;
        }
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;

        if (cooldownFillImage != null)
        {
            float cooldownProgress = PlayerController.Instance.GetShieldCooldownProgress();
            if (cooldownProgress > 0f)
            {
                cooldownFillImage.fillAmount = 1f - cooldownProgress;
            }
            else
            {
                cooldownFillImage.fillAmount = 0f;
            }
        }

        UpdateInteractable();

        if (cooldownText != null)
        {
            float remaining = PlayerController.Instance.GetShieldCooldownRemaining();
            if (remaining > 0f && showCooldownTimer)
            {
                cooldownText.text = Mathf.CeilToInt(remaining).ToString();
            }
            else if (!skillButton.interactable)
            {
                int cost = PlayerController.Instance.GetShieldManaCost();
                cooldownText.text = $"{cost}MP";
            }
            else
            {
                cooldownText.text = "";
            }
        }
    }

    private void OnSkillButtonClicked()
    {
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.TryActivateShield();
        }
    }

    private void UpdateInteractable()
    {
        if (skillButton == null || PlayerController.Instance == null) return;

        bool hasMana = PlayerMana.Instance != null && PlayerMana.Instance.HasEnoughMana(PlayerController.Instance.GetShieldManaCost());
        bool onCooldown = PlayerController.Instance.IsShieldOnCooldown();
        bool active = PlayerController.Instance.IsShieldActive();

        skillButton.interactable = hasMana || onCooldown || active;
    }

    private void UpdateCooldownUI(float cooldownProgress)
    {
        UpdateInteractable();
    }

    private void UpdateSkillStateUI(bool isActive)
    {
    }
}
