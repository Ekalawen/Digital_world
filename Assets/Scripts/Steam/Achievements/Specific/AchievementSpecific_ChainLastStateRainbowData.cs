using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_ChainLastStateRainbowData : Achievement_FinishLevel {

    public int nbToChain = 6;

    protected int currentNb = 0;

    protected override void InitializeSpecific() {
        gm.eventManager.onCaptureLumiere.AddListener(CheckChainEnough);
    }

    protected void CheckChainEnough(Lumiere lumiere) {
        LumiereEscape rainbowData = lumiere.GetComponent<LumiereEscape>();
        if (rainbowData != null && rainbowData.GetCurrentLives() == 0) {
            currentNb++;
            if(currentNb >= nbToChain) {
                Unlock();
            }
        } else {
            currentNb = 0;
        }
    }
}
