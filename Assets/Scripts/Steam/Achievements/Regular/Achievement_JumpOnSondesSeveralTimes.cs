using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Achievement_JumpOnSondesSeveralTimes : Achievement {

    [Header("Parameters")]
    public int nbTimesToJumpOnSonde = 5;

    protected int currentNbJumps = 0;

    protected override void InitializeSpecific() {
        gm.player.onHitBySonde.AddListener(IncrementCount);
        gm.player.onLand.AddListener(ResetCountIfDontLandOnSonde);
        gm.player.onGrip.AddListener(ResetCount);
    }

    protected void IncrementCount() {
        currentNbJumps++;
        if(currentNbJumps >= nbTimesToJumpOnSonde) {
            Unlock();
        }
    }

    protected void ResetCount() {
        currentNbJumps = 0;
    }

    protected void ResetCountIfDontLandOnSonde(GameObject thingLandedOn) {
        if(thingLandedOn.GetComponent<Sonde>() == null) {
            ResetCount();
        }
    }
}
