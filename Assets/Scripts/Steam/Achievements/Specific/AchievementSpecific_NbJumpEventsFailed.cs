using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbJumpEventsFailed : Achievement_FinishLevel {

    public int treshold;

    protected int currentNbJumps = 0;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.eventManager.onJumpFailed.AddListener(IncrementCounter);
    }

    protected void IncrementCounter() {
        currentNbJumps++;
    }

    public override void UnlockSpecific() {
        if (currentNbJumps <= treshold) {
            Unlock();
        }
    }
}
