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

    protected List<Ennemi> ennemisAlreadyHitten = new List<Ennemi>();
    protected ChargeCooldown chargeCooldown;

    public override void Initialize() {
        base.Initialize();
        chargeCooldown = GetComponent<ChargeCooldown>();
    }

    protected override bool UsePouvoir() {
        bool returnValue = base.UsePouvoir();
        ennemisAlreadyHitten.Clear();
        player.SetPowerDashingFor(dureePowerDahsing);
        return returnValue;
    }

    public bool HitEnnemy(Ennemi ennemi) {
        if(CanHitEnnemy(ennemi)) {
            GainChargeIfFirstHit();
            ennemisAlreadyHitten.Add(ennemi);
            ennemi.HitByPlayerPowerDash(this);
            return true;
        }
        return false;
    }

    protected void GainChargeIfFirstHit() {
        if(ennemisAlreadyHitten.Count == 0 && chargeCooldown != null) {
            chargeCooldown.GainCharge();
        }
    }

    public bool CanHitEnnemy(Ennemi ennemi) {
        return !ennemisAlreadyHitten.Contains(ennemi);
    }
}
