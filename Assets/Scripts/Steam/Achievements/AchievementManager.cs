using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using UnityEngine.Events;

public class AchievementManager : MonoBehaviour {

    public List<Achievement.Tag> tags;

    protected GameManager gm;
    protected SelectorManager sm;
    protected bool isInGame;
    protected List<Achievement> achievements = new List<Achievement>();
    [HideInInspector]
    public UnityEvent<Achievement> onUnlockAchievement;

    public void Initialize(bool isInGame) {
        InitMainManager(isInGame);
        RequestSteamCurrentStats();

        GatherRelevantAchievementsIn(transform, achievements);
        InitializeAllAchievements();
    }

    protected void RequestSteamCurrentStats() {
        if (SteamManager.Initialized) {
            SteamUserStats.RequestCurrentStats();
        }
    }

    protected void InitMainManager(bool isInGame) {
        this.isInGame = isInGame;
        if (isInGame) {
            gm = GameManager.Instance;
        } else {
            sm = SelectorManager.Instance;
        }
    }

    protected void GatherRelevantAchievementsIn(Transform folder, List<Achievement> list) {
        foreach(Transform t in folder) {
            Achievement achievement = t.GetComponent<Achievement>();
            if(achievement != null && achievement.IsRelevant(this)) {
                list.Add(achievement);
            } else {
                GatherRelevantAchievementsIn(t, list);
            }
        }
    }

    protected void GatherAllAchievementsIn(Transform folder, List<Achievement> list) {
        foreach(Transform t in folder) {
            Achievement achievement = t.GetComponent<Achievement>();
            if(achievement != null) {
                list.Add(achievement);
            } else {
                GatherAllAchievementsIn(t, list);
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
        foreach (Achievement achievement in GetAllAchievements()) {
            achievement.Unlock();
        }
    }

    public void LockAll() {
        WarnToUsePlayMode();
        Debug.Log($"LOCKING all achievements !");
        foreach (Achievement achievement in GetAllAchievements()) {
            achievement.Lock();
        }
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

    public List<Achievement> GetAllAchievements() {
        List<Achievement> allAchievements = new List<Achievement>();
        GatherAllAchievementsIn(transform, allAchievements);
        return allAchievements;
    }
}
