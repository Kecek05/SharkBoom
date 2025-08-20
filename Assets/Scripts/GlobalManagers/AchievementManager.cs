using System;
using UnityEngine;

[Serializable]
public class AchievementData
{
    public string Name;
    public string Id;
}


public static class AchievementManager
{
    public static void UnlockAchievement(AchievementData achievementData)
    {
        #if UNITY_ANDROID
        Social.ReportProgress(achievementData.Id, 100.0f, (bool success) => {
            // handle success or failure
            Debug.Log($"Gain Achievement {achievementData.Name}: {success}");
        });
        #endif
    }
}