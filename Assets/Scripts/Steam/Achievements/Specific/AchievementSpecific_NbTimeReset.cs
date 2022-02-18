using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbTimeReset : Achievement_FinishLevel {

    public int nbTimeReset;

    protected int nbTimeResetUsed = 0;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.itemManager.onTimeResetCatch.AddListener(IncrementCounter);
    }

    protected void IncrementCounter() {
        nbTimeResetUsed++;
    }

    public override void UnlockSpecific() {
        if (nbTimeResetUsed <= nbTimeReset) {
            Unlock();
        }
    }
}
