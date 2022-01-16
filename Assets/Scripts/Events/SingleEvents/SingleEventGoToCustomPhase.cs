using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleEventGoToCustomPhase : SingleEvent {

    public int phaseIndice = 1;

    public override void TriggerSpecific() {
        gm.timerManager.CustomGoToPhase(phaseIndice);
    }
}
