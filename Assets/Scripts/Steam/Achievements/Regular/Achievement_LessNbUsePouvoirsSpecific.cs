using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_LessNbUsePouvoirsSpecific : Achievement_FinishLevel {

    public PouvoirDisplay.PouvoirType pouvoirType = PouvoirDisplay.PouvoirType.DASH;
    public int treshold = 1;

    protected Timer timer;

    protected int nbUse = 0;

    protected override void InitializeSpecific() {
        base.InitializeSpecific();
        gm.player.onUsePouvoir.AddListener(OnUsePouvoir);
    }

    protected void OnUsePouvoir(IPouvoir pouvoir) {
        if(pouvoir.pouvoirType == pouvoirType) {
            nbUse++;
        }
    }

    public override void UnlockSpecific() {
        if(nbUse <= treshold) {
            Unlock();
        }
    }
}
