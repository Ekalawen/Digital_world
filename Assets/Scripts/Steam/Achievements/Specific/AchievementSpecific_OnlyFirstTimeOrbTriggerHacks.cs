using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_OnlyFirstTimeOrbTriggerHacks : Achievement_FinishLevel {

    protected bool hasFailed = false;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.itemManager.onOrbTriggerExit.AddListener(CheckHasFail);
    }

    protected void CheckHasFail(OrbTrigger orbTrigger) {
        if(!orbTrigger.IsDestroying()) {
            hasFailed = true;
        }
    }

    public override void UnlockSpecific() {
        if (!hasFailed) {
            Unlock();
        }
    }
}
