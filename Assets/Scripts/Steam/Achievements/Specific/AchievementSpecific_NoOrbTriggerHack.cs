using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NoOrbTriggerHack : Achievement_FinishLevel {

    protected bool hasFailed = false;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.itemManager.onOrbTriggerHacked.AddListener(SetHasFail);
    }

    protected void SetHasFail(OrbTrigger orbTrigger) {
        hasFailed = true;
    }

    public override void UnlockSpecific() {
        if (!hasFailed) {
            Unlock();
        }
    }
}
