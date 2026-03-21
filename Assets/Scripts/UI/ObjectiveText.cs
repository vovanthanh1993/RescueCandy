using UnityEngine;
using TMPro;

public class ObjectiveText : MonoBehaviour
{
    [Header("Quest1")]
    [SerializeField] private GameObject quest1Object;
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private string rescueMessage = "Rescue all your friends!";

    [Header("Quest2")]
    [SerializeField] private GameObject quest2Object;

    private bool isCompleted = false;

    private void OnEnable()
    {
        ResetQuests();
    }

    private void Update()
    {
        UpdateText();
    }

    public void ResetQuests()
    {
        isCompleted = false;
        if (quest1Object != null) quest1Object.SetActive(true);
        if (quest2Object != null) quest2Object.SetActive(false);
        UpdateText();
    }

    private void UpdateText()
    {
        if (LevelManager.Instance == null) return;

        int rescued = LevelManager.Instance.GetRescuedSweetieCount();
        int required = LevelManager.Instance.GetRequiredSweetieRescuesForCurrentLevel();

        if (required > 0 && rescued >= required)
        {
            if (!isCompleted)
            {
                isCompleted = true;
                if (quest2Object != null) quest2Object.SetActive(true);
                if (quest1Object != null) quest1Object.SetActive(false);
            }
        }
        else
        {
            if (isCompleted)
            {
                isCompleted = false;
                if (quest1Object != null) quest1Object.SetActive(true);
                if (quest2Object != null) quest2Object.SetActive(false);
            }
            if (objectiveText != null)
                objectiveText.text = $"{rescueMessage} ({rescued}/{required})";
        }
    }
}
