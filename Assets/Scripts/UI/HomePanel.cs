using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
public class HomePanel : MonoBehaviour
{
    public Button playBtn;
    public Button upgradeBtn;
    public TextMeshProUGUI coinText;

    void Start()
    {
        playBtn.onClick.AddListener(OnPlayButtonClicked);
        if (upgradeBtn != null)
            upgradeBtn.onClick.AddListener(OnUpgradeButtonClicked);
        UpdateRewardDisplay();
    }

    void OnPlayButtonClicked()
    {
        AudioManager.Instance.PlayPopupSound();
        UIManager.Instance.ShowHomePanel(false);
        GameCommonUtils.LoadScene("SelectLevel");
    }

    void OnUpgradeButtonClicked()
    {
        UIManager.Instance.ShowUpgradePanel(true);
        AudioManager.Instance.PlayPopupSound();
    }

    private void OnEnable() {
        UIManager.Instance.ShowGamePlayPanel(false);
        UpdateRewardDisplay();
    }

    /// <summary>
    /// Cập nhật hiển thị reward từ PlayerData
    /// </summary>
    public void UpdateRewardDisplay()
    {
        if (coinText != null)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
            {
                coinText.text = PlayerDataManager.Instance.playerData.totalReward.ToString();
            }
            else
            {
                coinText.text = "0";
            }
        }
    }

}
