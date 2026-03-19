using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Quest Data")]
public class QuestData : ScriptableObject
{
    public int questId;

    public QuestObjective[] objectives;

    public float timeFor3Stars = 60f;
    public float timeFor2Stars = 120f;
    
    [Header("Time Limit")]
    public float timeLimit = 300f;

    [Header("Sweetie Settings")]
    [Tooltip("Số Sweetie cần giải cứu để qua cổng (0 = không yêu cầu)")]
    public int requiredSweetieRescues = 0;

    [Header("Reward List")]
    [Tooltip("Reward cho 1 sao, 2 sao, 3 sao")]
    public List<int> rewardList = new List<int> { 50, 100, 150 };
}