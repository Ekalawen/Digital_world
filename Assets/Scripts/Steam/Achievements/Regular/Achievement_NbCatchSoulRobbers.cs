using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_NbCatchSoulRobbers : Achievement {

    [Header("Parameters")]
    public int treshold = 100;

    protected override void InitializeSpecific() {
        gm.ennemiManager.onCatchSoulRobber.AddListener(OnCatchSoulRobber);
    }

    protected void OnCatchSoulRobber(SoulRobber soulRobber) {
        if(PrefsManager.GetInt(PrefsManager.TOTAL_CATCH_SOULROBBER_KEY, 0) >= treshold) {
            Unlock();
        }
    }
}
