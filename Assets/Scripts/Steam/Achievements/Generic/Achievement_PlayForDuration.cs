using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_PlayForDuration : Achievement {

    [Header("Parameters")]
    public float duration = 600;

    protected override void InitializeSpecific() {
        gm.eventManager.onGameOver.AddListener(OnGameOver);
    }

    protected void OnGameOver() {
        if(gm.timerManager.GetRealElapsedTime() >= duration) {
            Unlock();
        }
    }
}
