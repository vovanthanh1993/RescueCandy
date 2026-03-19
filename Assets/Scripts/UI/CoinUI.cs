using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị số coin (totalReward) trong GamePlayPanel.
/// Gắn vào object Coin, tự tìm TextMeshProUGUI con nếu chưa gán.
/// </summary>
public class CoinUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinText;

    private int lastDisplayed = -1;

    private void Awake()
    {
        if (coinText == null)
            coinText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        lastDisplayed = -1; // force refresh
        UpdateCoinDisplay();
    }

    private void Update()
    {
        UpdateCoinDisplay();
    }

    private void UpdateCoinDisplay()
    {
        if (coinText == null) return;

        int current = 0;
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            current = PlayerDataManager.Instance.playerData.totalReward;

        if (current != lastDisplayed)
        {
            coinText.text = current.ToString();
            lastDisplayed = current;
        }
    }
}
