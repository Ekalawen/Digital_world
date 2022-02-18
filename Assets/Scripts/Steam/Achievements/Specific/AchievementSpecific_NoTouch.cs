using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NoTouch : Achievement_FinishLevel {

    public int nbTouch = 0;
    public bool withoutCheatCodes = false;

    protected int currentNbTouch = 0;
    protected bool hasUsedCheatCode = false;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.player.RegisterOnHit(IncrementCounter);
        if (withoutCheatCodes) {
            gm.cheatCodeManager.onUseCheatCode.AddListener(HasUseCheatCode);
        }
    }

    protected void HasUseCheatCode() {
        hasUsedCheatCode = true;
    }

    protected void IncrementCounter() {
        currentNbTouch++;
    }

    public override void UnlockSpecific() {
        if (currentNbTouch == nbTouch && (!withoutCheatCodes || !hasUsedCheatCode)) {
            Unlock();
        }
    }
}
