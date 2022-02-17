using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_RetryLevel : Achievement {

    [Header("Parameters")]
    public int treshold = 10;

    protected override void InitializeSpecific() {
        if(!gm.eventManager.HasAlreadyWin() && gm.eventManager.GetNbDeath() >= treshold) {
            Unlock();
        }
    }
}
