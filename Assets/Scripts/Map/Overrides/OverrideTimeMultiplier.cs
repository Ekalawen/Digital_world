using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class OverrideTimeMultiplier : Override {

    public TimeMultiplier timeMultiplier;

    protected override void InitializeSpecific() {
        gm.timerManager.AddTimeMultiplier(timeMultiplier);
    }
}
