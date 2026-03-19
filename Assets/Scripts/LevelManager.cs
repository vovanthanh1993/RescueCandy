using UnityEngine;

/// <summary>
/// Quản lý việc load level prefab từ Resources
/// và progress giải cứu Sweetie cho từng level
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    
    [Header("Level Settings")]
    [Tooltip("Parent object để chứa level prefab (nếu null sẽ tạo mới)")]
    [SerializeField] private Transform levelParent;
    
    [Tooltip("Đường dẫn đến folder level trong Resources")]
    [SerializeField] private string levelResourcePath = "Level";

    private GameObject currentLevelInstance;
    private int rescuedSweetieCount = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (levelParent == null)
        {
            GameObject parentObj = new GameObject("LevelParent");
            levelParent = parentObj.transform;
        }
    }
    
    public GameObject LoadLevel(int levelNumber)
    {
        UnloadCurrentLevel();
        ResetProgress();
        
        int prefabNumber = ((levelNumber - 1) % 15) + 1;
        string prefabName = $"Level{prefabNumber}";
        string resourcePath = $"{levelResourcePath}/{prefabName}";
        
        GameObject levelPrefab = Resources.Load<GameObject>(resourcePath);
        
        if (levelPrefab == null)
        {
            Debug.LogError($"LevelManager: Không tìm thấy level prefab tại '{resourcePath}'!");
            return null;
        }
        
        currentLevelInstance = Instantiate(levelPrefab, levelParent);
        currentLevelInstance.name = prefabName;
        
        Debug.Log($"LevelManager: Đã load level {levelNumber} (prefab {prefabName})");
        
        return currentLevelInstance;
    }
    
    public void UnloadCurrentLevel()
    {
        if (currentLevelInstance != null)
        {
            Destroy(currentLevelInstance);
            currentLevelInstance = null;
        }
    }
    
    public GameObject GetCurrentLevel()
    {
        return currentLevelInstance;
    }
    
    public bool HasLevelLoaded()
    {
        return currentLevelInstance != null;
    }

    #region Sweetie Rescue Progress

    public void OnSweetieRescued()
    {
        rescuedSweetieCount++;
        Debug.Log($"LevelManager: Sweetie rescued! ({rescuedSweetieCount}/{GetRequiredSweetieRescuesForCurrentLevel()})");
    }

    public int GetRescuedSweetieCount()
    {
        return rescuedSweetieCount;
    }

    public int GetRequiredSweetieRescuesForCurrentLevel()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.currentQuest != null)
        {
            return Mathf.Max(0, QuestManager.Instance.currentQuest.requiredSweetieRescues);
        }
        return 0;
    }

    public bool HasRescuedEnoughSweeties()
    {
        int required = GetRequiredSweetieRescuesForCurrentLevel();
        if (required <= 0)
            return true;
        return rescuedSweetieCount >= required;
    }

    public void ResetSweetieProgress()
    {
        rescuedSweetieCount = 0;
    }

    #endregion

    public void ResetProgress()
    {
        ResetSweetieProgress();

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.ResetSpeedSkill();
        }
    }
}
