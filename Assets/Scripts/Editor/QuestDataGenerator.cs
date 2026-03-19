using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Editor tool để tự động generate quest data cho nhiều level
/// </summary>
public class QuestDataGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Quest Data")]
    public static void ShowWindow()
    {
        GetWindow<QuestDataGenerator>("Quest Data Generator");
    }
    
    [MenuItem("Tools/Generate 50 Levels (Quick)")]
    public static void Generate50LevelsQuick()
    {
        GenerateQuestDataWithPattern(1, 50, 15, 3, 1, 120f, 10f, 50, 100, 150, 25);
    }
    
    private int startLevel = 1;
    private int endLevel = 50;
    
    private int baseSweetieRescues = 3;
    private int sweetieRescuesIncrement = 1;
    
    private float baseTimeLimit = 120f;
    private float timeLimitIncrement = 30f;
    
    private int baseReward1Star = 50;
    private int baseReward2Star = 100;
    private int baseReward3Star = 150;
    private int rewardIncrement = 25;
    
    private void OnGUI()
    {
        GUILayout.Label("Quest Data Generator", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        startLevel = EditorGUILayout.IntField("Start Level", startLevel);
        endLevel = EditorGUILayout.IntField("End Level", endLevel);
        
        GUILayout.Space(10);
        GUILayout.Label("Sweetie Rescue Settings", EditorStyles.boldLabel);
        baseSweetieRescues = EditorGUILayout.IntField("Base Sweetie Rescues (Level 1)", baseSweetieRescues);
        sweetieRescuesIncrement = EditorGUILayout.IntField("Sweetie Increment Per Level", sweetieRescuesIncrement);
        
        GUILayout.Space(10);
        GUILayout.Label("Time Limit Settings", EditorStyles.boldLabel);
        baseTimeLimit = EditorGUILayout.FloatField("Base Time Limit (Level 1)", baseTimeLimit);
        timeLimitIncrement = EditorGUILayout.FloatField("Time Limit Increment Per Level", timeLimitIncrement);
        
        GUILayout.Space(10);
        GUILayout.Label("Reward Settings", EditorStyles.boldLabel);
        baseReward1Star = EditorGUILayout.IntField("Base Reward 1 Star", baseReward1Star);
        baseReward2Star = EditorGUILayout.IntField("Base Reward 2 Star", baseReward2Star);
        baseReward3Star = EditorGUILayout.IntField("Base Reward 3 Star", baseReward3Star);
        rewardIncrement = EditorGUILayout.IntField("Reward Increment Per Level", rewardIncrement);
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("Generate Quest Data", GUILayout.Height(40)))
        {
            GenerateQuestData();
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Script này sẽ tạo quest data cho các level từ " + startLevel + " đến " + endLevel + 
            " với:\n" +
            "- Số Sweetie cần giải cứu tăng dần\n" +
            "- Time Limit tăng dần\n" +
            "- Reward tăng dần",
            MessageType.Info
        );
    }
    
    private void GenerateQuestData()
    {
        GenerateQuestDataStatic(
            startLevel, endLevel,
            baseSweetieRescues, sweetieRescuesIncrement,
            baseTimeLimit, timeLimitIncrement,
            baseReward1Star, baseReward2Star, baseReward3Star, rewardIncrement
        );
    }
    
    private static void GenerateQuestDataStatic(
        int startLevel, int endLevel,
        int baseSweetieRescues, int sweetieRescuesIncrement,
        float baseTimeLimit, float timeLimitIncrement,
        int baseReward1Star, int baseReward2Star, int baseReward3Star, int rewardIncrement)
    {
        Dictionary<int, QuestData> quests = new Dictionary<int, QuestData>();
        
        for (int level = startLevel; level <= endLevel; level++)
        {
            QuestData quest = ScriptableObject.CreateInstance<QuestData>();
            quest.questId = level;
            quest.objectives = new QuestObjective[0];
            quest.requiredSweetieRescues = baseSweetieRescues + (level - 1) * sweetieRescuesIncrement;
            
            quest.timeLimit = baseTimeLimit + (level - 1) * timeLimitIncrement;
            quest.timeFor3Stars = quest.timeLimit * 0.4f;
            quest.timeFor2Stars = quest.timeLimit * 0.7f;
            
            quest.rewardList = new List<int>
            {
                baseReward1Star + (level - 1) * rewardIncrement,
                baseReward2Star + (level - 1) * rewardIncrement,
                baseReward3Star + (level - 1) * rewardIncrement
            };
            
            quests[level] = quest;
        }
        
        QuestDataStorage.SaveAllQuests(quests);
        
        Debug.Log($"QuestDataGenerator: Đã tạo quest data cho {quests.Count} levels (từ level {startLevel} đến {endLevel})");
        
        EditorUtility.DisplayDialog(
            "Success",
            $"Đã tạo quest data cho {quests.Count} levels!\n" +
            $"Sweetie Rescues: {baseSweetieRescues} → tăng dần\n" +
            $"File được lưu tại: {QuestDataStorage.GetQuestFilePath()}",
            "OK"
        );
    }
    
    private static void GenerateQuestDataWithPattern(
        int startLevel, int endLevel,
        int patternLength, int baseSweetieRescues, int sweetieRescuesIncrement,
        float baseTimeLimit, float timeLimitIncrement,
        int baseReward1Star, int baseReward2Star, int baseReward3Star, int rewardIncrement)
    {
        Dictionary<int, QuestData> quests = new Dictionary<int, QuestData>();
        Dictionary<int, QuestData> patternQuests = new Dictionary<int, QuestData>();
        
        for (int patternLevel = 1; patternLevel <= patternLength; patternLevel++)
        {
            QuestData quest = ScriptableObject.CreateInstance<QuestData>();
            quest.questId = patternLevel;
            quest.objectives = new QuestObjective[0];
            
            int sweeties = baseSweetieRescues + (patternLevel - 1) * sweetieRescuesIncrement;
            quest.requiredSweetieRescues = Mathf.Min(sweeties, 20);
            
            quest.timeLimit = baseTimeLimit + (patternLevel - 1) * timeLimitIncrement;
            quest.timeFor3Stars = quest.timeLimit * 0.4f;
            quest.timeFor2Stars = quest.timeLimit * 0.7f;
            
            quest.rewardList = new List<int>
            {
                baseReward1Star + (patternLevel - 1) * rewardIncrement,
                baseReward2Star + (patternLevel - 1) * rewardIncrement,
                baseReward3Star + (patternLevel - 1) * rewardIncrement
            };
            
            patternQuests[patternLevel] = quest;
        }
        
        for (int level = startLevel; level <= endLevel; level++)
        {
            int patternIndex = ((level - 1) % patternLength) + 1;
            QuestData patternQuest = patternQuests[patternIndex];
            
            QuestData quest = ScriptableObject.CreateInstance<QuestData>();
            quest.questId = level;
            quest.objectives = patternQuest.objectives;
            quest.requiredSweetieRescues = patternQuest.requiredSweetieRescues;
            quest.timeLimit = patternQuest.timeLimit;
            quest.timeFor3Stars = patternQuest.timeFor3Stars;
            quest.timeFor2Stars = patternQuest.timeFor2Stars;
            
            quest.rewardList = new List<int>
            {
                baseReward1Star + (level - 1) * rewardIncrement,
                baseReward2Star + (level - 1) * rewardIncrement,
                baseReward3Star + (level - 1) * rewardIncrement
            };
            
            quests[level] = quest;
        }
        
        QuestDataStorage.SaveAllQuests(quests);
        
        Debug.Log($"QuestDataGenerator: Đã tạo quest data cho {quests.Count} levels");

        EditorUtility.DisplayDialog(
            "Success",
            $"Đã tạo quest data cho {quests.Count} levels!\n" +
            $"Pattern: {patternLength} level, Sweetie: {baseSweetieRescues} → tối đa 20\n" +
            $"File được lưu tại: {QuestDataStorage.GetQuestFilePath()}",
            "OK"
        );
    }
}
