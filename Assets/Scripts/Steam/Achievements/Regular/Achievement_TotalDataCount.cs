using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_TotalDataCount : Achievement {

    [Header("Parameters")]
    public int treshold = 100;

    protected override void InitializeSpecific() {
        gm.eventManager.onCaptureLumiere.AddListener(UnlockIfBetterTreshold);
    }

    public void UnlockIfBetterTreshold() {
        if(Lumiere.GetTotalDataCount() >= treshold) {
            Unlock();
        }
    }
}
