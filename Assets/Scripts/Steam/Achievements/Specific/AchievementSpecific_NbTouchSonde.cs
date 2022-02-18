using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbTouchSonde : Achievement_FinishLevel {

    public int nbTouch = 1;

    protected int currentNbTouch = 0;
    protected bool hasUsedCheatCode = false;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.ennemiManager.onHitBySonde.AddListener(IncrementCounter);
        gm.cheatCodeManager.onUseCheatCode.AddListener(RememberUseCheatCode);
    }

    protected void IncrementCounter() {
        currentNbTouch++;
    }

    public void RememberUseCheatCode() {
        hasUsedCheatCode = true;
    }

    public override void UnlockSpecific() {
        if (currentNbTouch == nbTouch && !hasUsedCheatCode) {
            Unlock();
        }
    }
}
