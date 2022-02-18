using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_PushSeveralEnnemisWithPowerDash : Achievement {

    [Header("Parameters")]
    public int nbEnnemisToPush = 5;

    protected List<Ennemi> ennemisPushed = new List<Ennemi>();

    protected override void InitializeSpecific() {
        gm.player.onUseDash.AddListener(ResetSet);
        gm.player.onPowerDashEnnemiImpact.AddListener(OnPowerDashEnnemiImpact);
    }

    protected void ResetSet(PouvoirDash pouvoirDash) {
        ennemisPushed.Clear();
    }

    protected void OnPowerDashEnnemiImpact(PouvoirPowerDash powerDash, Ennemi ennemi) {
        if (!ennemisPushed.Contains(ennemi)) {
            ennemisPushed.Add(ennemi);
        }
        if(ennemisPushed.Count >= nbEnnemisToPush) {
            Unlock();
        }
    }
}
