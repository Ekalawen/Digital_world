using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.SceneManagement;

public class AchievementSpecific_NbDatasInOneGame : Achievement_FinishLevel {

    public int treshold = 300;

    protected int currentNb = 0;

    protected override void InitializeSpecific() {
        gm.eventManager.onCaptureLumiere.AddListener(IncrementCounter);
    }

    protected void IncrementCounter(Lumiere lumiere) {
        currentNb++;
        if(currentNb >= treshold) {
            Unlock();
        }
    }
}
