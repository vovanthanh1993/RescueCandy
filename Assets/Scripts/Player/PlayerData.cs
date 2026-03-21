using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int totalReward = 0;

    public int bonusHealth = 0;
    public int bonusMana = 0;
    public int bonusDamage = 0;

    public static PlayerData CreateDefault(int totalLevels = 50)
    {
        PlayerData data = new PlayerData();
        data.totalReward = 0;
        data.bonusHealth = 0;
        data.bonusMana = 0;
        data.bonusDamage = 0;

        return data;
    }
}

[Serializable]
public class PlayerLevelData
{
    public int level;
    public int star;
    public bool isLocked;
}