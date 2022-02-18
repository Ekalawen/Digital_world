using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_NbUsePouvoirs : Achievement {

    [Header("Parameters")]
    public PouvoirDisplay.PouvoirType pouvoirType = PouvoirDisplay.PouvoirType.DASH;
    public int treshold = 100;

    protected Timer timer;

    protected int nbUse = 0;

    protected override void InitializeSpecific() {
        gm.player.onUsePouvoir.AddListener(OnUsePouvoir);
    }

    protected void OnUsePouvoir(IPouvoir pouvoir) {
        if(pouvoir.pouvoirType == pouvoirType) {
            nbUse++;
            if(nbUse >= treshold) {
                Unlock();
            }
        }
    }
}
