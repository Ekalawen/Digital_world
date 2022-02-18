using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class Achievement_DieQuickerThanDuration : Achievement {

    [Header("Parameters")]
    public float duration = 3.0f;

    protected override void InitializeSpecific() {
        gm.eventManager.onLoseGame.AddListener(OnLose);
    }

    protected void OnLose(EventManager.DeathReason reason) {
        if(gm.timerManager.GetRealElapsedTime() <= duration) {
            Unlock();
        }
    }
}
