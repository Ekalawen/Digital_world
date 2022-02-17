using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public abstract class Achievement : MonoBehaviour {

    public enum Tag {
        GENERIC,
        REGULAR,
        IR,
    };

    [Header("Main parameters")]
    public string id;

    [Header("Condition")]
    public bool isInGame = true;
    public List<Tag> tags = new List<Tag>() { Tag.GENERIC };

    protected GameManager gm;
    protected SelectorManager sm;
    protected AchievementManager achievementManager;

    public void Initialize(AchievementManager achievementManager) {
        if(isInGame) {
            gm = GameManager.Instance;
        } else {
            sm = SelectorManager.Instance;
        }
        this.achievementManager = achievementManager;
        InitializeSpecific();
    }

    protected abstract void InitializeSpecific();

    public virtual bool IsRelevant(AchievementManager achievementManager) {
        return isInGame == achievementManager.IsInGame()
            && tags.All(t => achievementManager.GetTags().Contains(t))
            && !IsUnlocked();
    }

    public void Unlock() {
        if(IsUnlocked()) {
            return;
        }
        if (SteamManager.Initialized) {
            bool succeed = SteamUserStats.SetAchievement(id);
            if(succeed) {
                PrefsManager.SetBool(id, true);
                SteamUserStats.StoreStats();
                Debug.Log($"Unlocking achievement {id} completed!");
            } else {
                Debug.Log($"Unlocking achievement {id} failed!");
            }
        } else {
            Debug.Log($"Achievement {id} will not be unlocked because the SteamManager is not Initialized.");
        }
    }

    public void Lock() {
        if(IsLocked()) {
            Debug.Log($"Achievement {id} is already locked.");
            return;
        }
        if (SteamManager.Initialized) {
            bool succeed = SteamUserStats.ClearAchievement(id);
            if(succeed) {
                PrefsManager.SetBool(id, false);
                SteamUserStats.StoreStats();
                Debug.Log($"Locking achievement {id} completed!");
            } else {
                Debug.Log($"Locking achievement {id} failed!");
            }
        } else {
            Debug.Log($"Achievement {id} will not be locked because the SteamManager is not Initialized.");
        }
    }

    public bool IsUnlocked() {
        return PrefsManager.GetBool(id, false);
    }

    public bool IsLocked() {
        return !IsUnlocked(); 
    }
}
