using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_FinishLevel : Achievement {

    [Header("Parameters")]
    public string levelSceneName = "levelNameScene";

    protected override void InitializeSpecific() {
        gm.eventManager.onWinGame.AddListener(Unlock);
    }

    public override bool IsRelevant(AchievementManager achievementManager) {
        return base.IsRelevant(achievementManager) && SceneManager.GetActiveScene().name == levelSceneName;
    }
}
