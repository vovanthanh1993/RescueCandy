using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradePanel : MonoBehaviour
{
    [Header("Coin Display")]
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Health Upgrade")]
    [SerializeField] private Button healthUpgradeBtn;
    [SerializeField] private int healthUpgradeAmount = 5;
    [SerializeField] private int healthUpgradeCost = 600;

    [Header("Mana Upgrade")]
    [SerializeField] private Button manaUpgradeBtn;
    [SerializeField] private int manaUpgradeAmount = 5;
    [SerializeField] private int manaUpgradeCost = 400;

    [Header("Damage Upgrade")]
    [SerializeField] private Button damageUpgradeBtn;
    [SerializeField] private int damageUpgradeAmount = 5;
    [SerializeField] private int damageUpgradeCost = 800;

    [Header("Close")]
    [SerializeField] private Button closeBtn;

    private void Start()
    {
        if (healthUpgradeBtn != null)
            healthUpgradeBtn.onClick.AddListener(OnHealthUpgrade);
        if (manaUpgradeBtn != null)
            manaUpgradeBtn.onClick.AddListener(OnManaUpgrade);
        if (damageUpgradeBtn != null)
            damageUpgradeBtn.onClick.AddListener(OnDamageUpgrade);
        if (closeBtn != null)
            closeBtn.onClick.AddListener(Close);

    }

    private void OnEnable()
    {
        UpdateCoinDisplay();
    }

    private void OnHealthUpgrade()
    {
        if (!TrySpendCoin(healthUpgradeCost)) return;

        PlayerDataManager.Instance.playerData.bonusHealth += healthUpgradeAmount;
        PlayerDataManager.Instance.Save();

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.AddMaxHealth(healthUpgradeAmount);

        int totalHealth = PlayerHealth.Instance != null ? PlayerHealth.Instance.MaxHealth : 0;
        ShowNotice($"Upgrade success!\nHealth: {totalHealth} (+{healthUpgradeAmount})");
        UpdateCoinDisplay();
    }

    private void OnManaUpgrade()
    {
        if (!TrySpendCoin(manaUpgradeCost)) return;

        PlayerDataManager.Instance.playerData.bonusMana += manaUpgradeAmount;
        PlayerDataManager.Instance.Save();

        if (PlayerMana.Instance != null)
            PlayerMana.Instance.AddMaxMana(manaUpgradeAmount);

        int totalMana = PlayerMana.Instance != null ? PlayerMana.Instance.MaxMana : 0;
        ShowNotice($"Upgrade success!\nMana: {totalMana} (+{manaUpgradeAmount})");
        UpdateCoinDisplay();
    }

    private void OnDamageUpgrade()
    {
        if (!TrySpendCoin(damageUpgradeCost)) return;

        PlayerDataManager.Instance.playerData.bonusDamage += damageUpgradeAmount;
        PlayerDataManager.Instance.Save();

        if (PlayerCombat.Instance != null)
            PlayerCombat.Instance.AddDamage(damageUpgradeAmount);

        int totalDamage = PlayerCombat.Instance != null ? PlayerCombat.Instance.GetPlayerDamage() : 0;
        ShowNotice($"Upgrade success!\nDamage: {totalDamage} (+{damageUpgradeAmount})");
        UpdateCoinDisplay();
    }

    private bool TrySpendCoin(int cost)
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null)
            return false;

        if (PlayerDataManager.Instance.playerData.totalReward < cost)
        {
            ShowNotice("Insufficient gold!");
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFailSound();
            return false;
        }

        PlayerDataManager.Instance.playerData.totalReward -= cost;
        PlayerDataManager.Instance.Save();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySuccessSound();

        return true;
    }

    private void UpdateCoinDisplay()
    {
        if (coinText == null) return;
        int coins = 0;
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            coins = PlayerDataManager.Instance.playerData.totalReward;
        coinText.text = coins.ToString();
    }

    private void ShowNotice(string message)
    {
        if (UIManager.Instance != null && UIManager.Instance.noticePanel != null)
            UIManager.Instance.noticePanel.Init(message);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayCloseSound();
    }
}
