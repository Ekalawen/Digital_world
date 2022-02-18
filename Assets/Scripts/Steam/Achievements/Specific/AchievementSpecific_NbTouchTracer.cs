using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbTouchTracer : Achievement_FinishLevel {

    public int nbTouch = 1;

    protected int currentNbTouch = 0;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.player.onHitByTracer.AddListener(IncrementCounter);
    }

    protected void IncrementCounter() {
        currentNbTouch++;
    }

    public override void UnlockSpecific() {
        if (currentNbTouch == nbTouch) {
            Unlock();
        }
    }
}
