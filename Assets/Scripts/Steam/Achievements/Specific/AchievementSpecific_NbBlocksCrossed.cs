using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbBlocksCrossed : Achievement_FinishLevelIR {

    public int treshold = 100;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.eventManager.onGameOver.AddListener(UnlockSpecific);
    }

    public override void UnlockSpecific() {
        if (map.GetNonStartNbBlocksRun() >= treshold) {
            Unlock();
        }
    }
}
