using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class JumpButton : MonoBehaviour, IPointerDownHandler
{
    [Header("Cooldown UI")]
    [SerializeField] private Image cooldownFillImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private bool showCooldownTimer = true;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PlayerController.Instance != null && PlayerController.Instance.IsJumpOnCooldown()) return;
        if (InputManager.Instance != null)
            InputManager.Instance.OnJumpButtonDown();
    }

    private void Update()
    {
        if (PlayerController.Instance == null) return;

        if (cooldownFillImage != null)
        {
            float progress = PlayerController.Instance.GetJumpCooldownProgress();
            cooldownFillImage.fillAmount = progress > 0f ? 1f - progress : 0f;
        }

        if (showCooldownTimer && cooldownText != null)
        {
            float remaining = PlayerController.Instance.GetJumpCooldownRemaining();
            cooldownText.text = remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : "";
        }
    }
}
