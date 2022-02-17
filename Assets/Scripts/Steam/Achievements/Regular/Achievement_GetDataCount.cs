using Steamworks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_GetDataCount : Achievement {

    [Header("Parameters")]
    public int treshold = 100;

    protected override void InitializeSpecific() {
        gm.eventManager.onCaptureLumiere.AddListener(UnlockIfBetterTreshold);
    }

    public void UnlockIfBetterTreshold(Lumiere lumiere) {
        if(Lumiere.GetCurrentDataCount() >= treshold) {
            Unlock();
        }
    }
}
