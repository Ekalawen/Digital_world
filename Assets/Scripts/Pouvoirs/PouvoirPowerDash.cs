using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirPowerDash : PouvoirDash {

    [Header("Impact Effect")]
    public float dureePowerDahsing = 0.5f;
    public float dureePoussee = 0.5f;
    public float distancePoussee = 10f;
    public SpeedMultiplier speedMultiplierStun;

    protected override bool UsePouvoir() {
        bool returnValue = base.UsePouvoir();
        player.SetPowerDashingFor(dureePowerDahsing);
        return returnValue;
    }
}
