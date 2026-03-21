using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public GameObject introPanel;
    public HomePanel homePanel;

    public GameObject selectLevelPanel;

    public StartPanel startPanel;

    public GamePlayPanel gamePlayPanel;

    public GameObject loadingPanel;

    public NoticePanel noticePanel;

    public SettingPanel settingPanel;

    public GameObject upgradePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowLoadingPanel(bool isShow) {
        if (loadingPanel != null)
        {
            loadingPanel.gameObject.SetActive(isShow);
        }
    }

    public void ShowSelectLevelPanel(bool isShow) {
        if (selectLevelPanel != null)
        {
            selectLevelPanel.SetActive(isShow);
            
            // Refresh SelectLevelPanel khi panel được hiển thị
            if (isShow)
            {
                SelectLevelPanel selectLevelPanelComponent = selectLevelPanel.GetComponentInChildren<SelectLevelPanel>();
                if (selectLevelPanelComponent != null)
                {
                    selectLevelPanelComponent.Refresh();
                }
            }
        }
    }

    public void ShowGamePlayPanel(bool isShow) {
        if (gamePlayPanel != null)
        {
            gamePlayPanel.gameObject.SetActive(isShow);
        }
    }

    public void ShowIntroPanel(bool isShow) {
        if (introPanel != null)
        {
            introPanel.SetActive(isShow);
        }
    }

    public void ShowHomePanel(bool isShow) {
        if (homePanel != null)
        {
            homePanel.gameObject.SetActive(isShow);
        }
    }

    private void OnEnable() {
        selectLevelPanel.gameObject.SetActive(false);
        gamePlayPanel.gameObject.SetActive(false);
        introPanel.SetActive(true);
        homePanel.gameObject.SetActive(false);
        noticePanel.gameObject.SetActive(false);
        settingPanel.gameObject.SetActive(false);
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    private void Update()
    {
        // Nhấn F1 để unlock tất cả level (cheat code)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UnlockAllLevels();
        }
        
        // Nhấn F2 để reset tất cả về level 1 (cheat code)
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ResetAllLevels();
        }

        // Nhấn F3 để thêm 1000 vàng (cheat code)
        if (Input.GetKeyDown(KeyCode.F3))
        {
            AddCheatGold();
        }

        // Nhấn F4 để reset chỉ số về ban đầu (cheat code)
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ResetUpgradeStats();
        }
    }
    
    /// <summary>
    /// Unlock tất cả level (cheat code F1)
    /// </summary>
    private void UnlockAllLevels()
    {
        // Đếm số level đã unlock trước đó
        Dictionary<int, QuestData> allQuests = QuestDataStorage.LoadAllQuests();
        int lockedCountBefore = 0;
        foreach (var quest in allQuests.Values)
        {
            if (QuestDataStorage.IsQuestLocked(quest.questId))
            {
                lockedCountBefore++;
            }
        }
        
        // Unlock tất cả quest
        QuestDataStorage.UnlockAllQuests();
        
        // Đếm lại số level đã unlock
        allQuests = QuestDataStorage.LoadAllQuests();
        int unlockedCount = 0;
        foreach (var quest in allQuests.Values)
        {
            if (!QuestDataStorage.IsQuestLocked(quest.questId))
            {
                unlockedCount++;
            }
        }
        
        // Refresh SelectLevelPanel nếu đang mở
        if (selectLevelPanel != null && selectLevelPanel.activeSelf)
        {
            SelectLevelPanel selectLevelPanelComponent = selectLevelPanel.GetComponentInChildren<SelectLevelPanel>();
            if (selectLevelPanelComponent != null)
            {
                selectLevelPanelComponent.Refresh();
            }
        }
        
        // Hiển thị thông báo
        string message = lockedCountBefore > 0 
            ? $"Đã mở khóa {lockedCountBefore} level!\nTổng cộng: {unlockedCount} level đã mở khóa."
            : $"Tất cả level đã được mở khóa!\nTổng cộng: {unlockedCount} level.";
        
        if (noticePanel != null)
        {
            noticePanel.Init(message);
        }
        
        Debug.Log($"Cheat Code F1: {message}");
    }
    
    /// <summary>
    /// Reset tất cả level về trạng thái ban đầu (cheat code F2)
    /// </summary>
    private void ResetAllLevels()
    {
        // Reset tất cả quest về trạng thái ban đầu
        QuestDataStorage.ResetAllQuests();
        
        // Refresh SelectLevelPanel nếu đang mở
        if (selectLevelPanel != null && selectLevelPanel.activeSelf)
        {
            SelectLevelPanel selectLevelPanelComponent = selectLevelPanel.GetComponentInChildren<SelectLevelPanel>();
            if (selectLevelPanelComponent != null)
            {
                selectLevelPanelComponent.Refresh();
            }
        }
        
        // Hiển thị thông báo
        string message = "Đã reset tất cả về trạng thái ban đầu!\nChỉ level 1 được mở khóa, tất cả stars đã được reset về 0.";
        
        if (noticePanel != null)
        {
            noticePanel.Init(message);
        }
        
        Debug.Log($"Cheat Code F2: {message}");
    }

    private void ResetUpgradeStats()
    {
        if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.playerData == null) return;

        PlayerDataManager.Instance.playerData.bonusHealth = 0;
        PlayerDataManager.Instance.playerData.bonusMana = 0;
        PlayerDataManager.Instance.playerData.bonusDamage = 0;
        PlayerDataManager.Instance.Save();

        if (PlayerHealth.Instance != null)
            PlayerHealth.Instance.ApplyBonusStats();
        if (PlayerMana.Instance != null)
            PlayerMana.Instance.ApplyBonusStats();
        if (PlayerCombat.Instance != null)
            PlayerCombat.Instance.ApplyBonusStats();

        if (noticePanel != null)
            noticePanel.Init("All stats reset to default!");

        Debug.Log("Cheat Code F4: All upgrade stats reset.");
    }

    private void AddCheatGold()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.playerData != null)
        {
            PlayerDataManager.Instance.playerData.totalReward += 1000;
            PlayerDataManager.Instance.Save();

            if (noticePanel != null)
                noticePanel.Init("+1000 Gold!\nTotal: " + PlayerDataManager.Instance.playerData.totalReward);

            Debug.Log($"Cheat Code F3: +1000 Gold. Total: {PlayerDataManager.Instance.playerData.totalReward}");
        }
    }

    public void ShowUpgradePanel(bool isShow) {
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(isShow);
        }
    }

    public void ShowSettingPanel(bool isShow) {
        if (settingPanel != null)
        {
            settingPanel.gameObject.SetActive(isShow);
        }
    }

}
