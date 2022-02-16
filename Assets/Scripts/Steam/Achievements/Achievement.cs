using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public abstract class Achievement : MonoBehaviour {

    public enum Tag {
        GENERIC,
    };

    [Header("Main parameters")]
    public string id;

    [Header("Condition")]
    public bool isInGame = true;
    public List<Tag> tags;

    public abstract void Initialize();

    public virtual bool IsRelevant(AchievementManager achievementManager) {
        return isInGame == achievementManager.IsInGame()
            && tags.All(t => achievementManager.GetTags().Contains(t))
            && !IsUnlocked();
    }

    public void Unlock() {
        if(IsUnlocked()) {
            Debug.Log($"Achievement {name} is already unlocked.");
        }
        if (SteamManager.Initialized) {
            Debug.Log($"Unlocking achievement {name} ...");
            bool succeed = SteamUserStats.SetAchievement(id);
            if(succeed) {
                PrefsManager.SetBool(id, true);
                Debug.Log($"Unlocking achievement {name} completed!");
            } else {
                Debug.Log($"Unlocking achievement {name} failed!");
            }
        } else {
            Debug.Log($"Achievement {name} will not be unlocked because the SteamManager is not Initialized.");
        }
    }

    public void Lock() {
        if(IsLocked()) {
            Debug.Log($"Achievement {name} is already locked.");
        }
        if (SteamManager.Initialized) {
            Debug.Log($"Locking achievement {name} ...");
            bool succeed = SteamUserStats.ClearAchievement(id);
            if(succeed) {
                PrefsManager.SetBool(id, false);
                Debug.Log($"Locking achievement {name} completed!");
            } else {
                Debug.Log($"Locking achievement {name} failed!");
            }
        } else {
            Debug.Log($"Achievement {name} will not be locked because the SteamManager is not Initialized.");
        }
    }

    public bool IsUnlocked() {
        // TODO !
        return false; 
    }

    public bool IsLocked() {
        return !IsUnlocked(); 
    }
}
