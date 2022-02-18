using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbTouchVoidCube : Achievement_FinishLevel {

    public int nbTouch = 0;

    protected int currentNbTouch = 0;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.ennemiManager.onVoidCubeHit.AddListener(IncrementCounter);
    }

    protected void IncrementCounter(VoidCube voidCube) {
        currentNbTouch++;
    }

    public override void UnlockSpecific() {
        if (currentNbTouch == nbTouch) {
            Unlock();
        }
    }
}
