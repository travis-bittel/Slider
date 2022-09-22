using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

/// <summary>
/// This class handles storing achievement data and sending it to steam. Use <see cref="SetAchievementStat(string, int)"/> to set/update a particular achievement stat.
/// You can also use <see cref="GetAchievementData"/> to get all stored achievement data. This is used for saving achievement data to save profiles.
/// </summary>
public class AchievementManager : Singleton<AchievementManager>
{
    private Dictionary<string, int> achievementStats;

    /// <summary>
    /// Set a statistic with the given key. This key needs to match one setup in Steamworks (message Daniel or Travis to have them create a statistic!)
    /// </summary>
    /// <param name="statName"></param>
    /// <param name="value"></param>
    public static void SetAchievementStat(string statName, int value)
    {
        _instance.achievementStats[statName] = value;
        _instance.SendAchievementStatsToSteam();
    }

    public static void SetAchievementStat(AchievementStatistic achievementStatistic)
    {
        _instance.achievementStats[achievementStatistic.Key] = achievementStatistic.Value;
        _instance.SendAchievementStatsToSteam();
    }

    private void SendAchievementStatsToSteam()
    {
        foreach (string key in achievementStats.Keys)
        {
            SteamUserStats.SetStat(key, achievementStats[key]);
        }
    }

    public static AchievementStatistic[] GetAchievementData()
    {
        AchievementStatistic[] pairs = new AchievementStatistic[_instance.achievementStats.Count];
        int i = 0;
        foreach (string key in _instance.achievementStats.Keys)
        {
            pairs[i] = new AchievementStatistic(key, _instance.achievementStats[key]);
            i++;
        }
        return pairs;
    }

    public static void OverwriteAchievementData(AchievementStatistic[] achievementStatistics)
    {
        foreach (AchievementStatistic statistic in achievementStatistics)
        {
            _instance.achievementStats[statistic.Key] = statistic.Value;
        }
    }

    private void Start()
    {
        InitializeSingleton();
        achievementStats = new Dictionary<string, int>();
    }
}

/// <summary>
/// Represents a key-value pair for a Steam achievement statistic.
/// </summary>
public struct AchievementStatistic
{
    public string Key { get; private set; }
    public int Value { get; private set; }

    public AchievementStatistic(string key, int value)
    {
        Key = key;
        Value = value;
    }
}
