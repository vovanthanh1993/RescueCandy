using UnityEngine;

/// <summary>
/// Helper class để tính toán scene name dựa trên level number
/// </summary>
public static class LevelSceneHelper
{
    /// <summary>
    /// Xác định tên scene dựa trên level
    /// Pattern: Level 1-5: GamePlay1, Level 6-10: GamePlay2, Level 11-15: GamePlay3, Level 16-20: GamePlay1 (lặp lại)
    /// </summary>
    /// <param name="level">Số level (1, 2, 3, ...)</param>
    /// <returns>Tên scene tương ứng</returns>
    public static string GetSceneNameForLevel(int level)
    {
        return "GamePlay1";
    }
}
