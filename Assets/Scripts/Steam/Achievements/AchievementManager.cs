using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class AchievementManager : MonoBehaviour {

    public List<Achievement.Tag> tags;

    protected GameManager gm;
    protected SelectorManager sm;
    protected bool isInGame;
    protected List<Achievement> achievements = new List<Achievement>();

    public void Initialize(bool isInGame) {
        InitMainManager(isInGame);
        RequestSteamCurrentStats();

        GatherRelevantAchievementsIn(transform);
        InitializeAllAchievements();
    }

    protected void RequestSteamCurrentStats() {
        SteamUserStats.RequestCurrentStats();
    }

    protected void InitMainManager(bool isInGame) {
        this.isInGame = isInGame;
        if (isInGame) {
            gm = GameManager.Instance;
        } else {
            sm = SelectorManager.Instance;
        }
    }

    protected void GatherRelevantAchievementsIn(Transform folder) {
        foreach(Transform t in folder) {
            Achievement achievement = t.GetComponent<Achievement>();
            if(achievement != null && achievement.IsRelevant(this)) {
                achievements.Add(achievement);
            } else {
                GatherRelevantAchievementsIn(t);
            }
        }
    }

    protected void GatherAllAchievementsIn(Transform folder) {
        foreach(Transform t in folder) {
            Achievement achievement = t.GetComponent<Achievement>();
            if(achievement != null) {
                achievements.Add(achievement);
            } else {
                GatherAllAchievementsIn(t);
            }
        }
    }

    protected void InitializeAllAchievements() {
        foreach(Achievement achievement in achievements) {
            achievement.Initialize(this);
        }
        Debug.Log($"{achievements.Count} achievements initialisés !");
    }

    public bool IsInGame() {
        return isInGame;
    }

    public List<Achievement.Tag> GetTags() {
        return tags;
    }

    public void UnlockAll() {
        WarnToUsePlayMode();
        Debug.Log($"UNLOCKING all achievements !");
        List<Achievement> oldAchievements = achievements;
        achievements = new List<Achievement>();
        GatherAllAchievementsIn(transform);
        foreach (Achievement achievement in achievements) {
            achievement.Unlock();
        }
        achievements = oldAchievements;
    }

    public void LockAll() {
        WarnToUsePlayMode();
        Debug.Log($"LOCKING all achievements !");
        List<Achievement> oldAchievements = achievements;
        achievements = new List<Achievement>();
        GatherAllAchievementsIn(transform);
        foreach (Achievement achievement in achievements) {
            achievement.Lock();
        }
        achievements = oldAchievements;
    }

    public void UnlockAllRelevant() {
        WarnToUsePlayMode();
        Debug.Log($"UNLOCKING all relevant achievements !");
        foreach (Achievement achievement in achievements) {
            achievement.Unlock();
        }
    }

    public void LockAllRelevant() {
        WarnToUsePlayMode();
        Debug.Log($"LOCKING all relevant achievements !");
        foreach (Achievement achievement in achievements) {
            achievement.Lock();
        }
    }

    protected void WarnToUsePlayMode() {
        if (achievements == null || achievements.Count == 0) {
            throw new Exception($"This action only works in play mode!");
        }
    }
}
