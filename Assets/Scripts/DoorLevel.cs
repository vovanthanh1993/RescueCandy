using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class DoorLevel : MonoBehaviour
{
    [Tooltip("Level tương ứng với door này")]
    [SerializeField] private int levelNumber = 1;

    [Header("UI Display")]
    [SerializeField] private TextMeshProUGUI levelText;
    [Tooltip("3 object sao (On) theo thứ tự sao 1, 2, 3")]
    [SerializeField] private List<GameObject> starObjects = new List<GameObject>();
    [Tooltip("Object hiện khi locked")]
    [SerializeField] private GameObject lockObject;
    [Tooltip("Object hiện khi unlocked (icon start/sao)")]
    [SerializeField] private GameObject startObject;

    public int GetLevelNumber() => levelNumber;

    private void Start()
    {
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        int stars = QuestDataStorage.GetQuestStars(levelNumber);
        bool locked = QuestDataStorage.IsQuestLocked(levelNumber);

        if (lockObject != null)
            lockObject.SetActive(locked);
        if (startObject != null)
            startObject.SetActive(!locked);

        if (levelText != null)
            levelText.text = levelNumber.ToString();

        for (int i = 0; i < starObjects.Count; i++)
        {
            if (starObjects[i] != null)
                starObjects[i].SetActive(!locked && i < stars);
        }
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;
        OpenStartPanel();
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    return;

                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
                {
                    OpenStartPanel();
                }
            }
        }
    }

    private void OpenStartPanel()
    {
        if (UIManager.Instance == null || UIManager.Instance.startPanel == null) return;

        int stars = QuestDataStorage.GetQuestStars(levelNumber);
        bool locked = QuestDataStorage.IsQuestLocked(levelNumber);

        PlayerLevelData levelData = new PlayerLevelData
        {
            level = levelNumber,
            star = stars,
            isLocked = locked
        };

        if (locked)
        {
            if (UIManager.Instance.noticePanel != null)
                UIManager.Instance.noticePanel.Init($"Level {levelNumber} is locked!");
            return;
        }

        UIManager.Instance.startPanel.ShowForLevel(levelData);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPopupSound();
    }
}
