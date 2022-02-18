using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_UnlockAllOtherAchievements : Achievement {

    protected override void InitializeSpecific() {
        achievementManager.onUnlockAchievement.AddListener(CheckAllAchievementsUnlocked);
    }

    protected void CheckAllAchievementsUnlocked(Achievement achievement) {
        if(AllOtherAchievementsAreUnlocked()) {
            Unlock();
        }
    }

    protected bool AllOtherAchievementsAreUnlocked() {
        List<Achievement> allOtherAchievements = achievementManager.GetAllAchievements().FindAll(a => a.id != id);
        return allOtherAchievements.All(a => a.IsUnlocked());
    }
}
