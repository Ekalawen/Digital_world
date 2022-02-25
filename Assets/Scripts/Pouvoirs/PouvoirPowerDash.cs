using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.VFX;

public class PouvoirPowerDash : PouvoirDash {

    [Header("Impact Effect")]
    public float dureePowerDahsing = 0.5f;
    public float dureePoussee = 0.5f;
    public float distancePoussee = 10f;
    public SpeedMultiplier speedMultiplierStun;

    [Header("Visual")]
    public GameObject impactVfxPrefab;
    public float coefFakeImpactPosition = 0.5f;
    public TimeMultiplier impactTimeMultiplier;
    public TimeMultiplier impactBrisableTimeMultiplier;

    protected List<Ennemi> ennemisAlreadyHitten = new List<Ennemi>();
    protected ChargeCooldown chargeCooldown;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected bool hasAlreadyGainChargeForBrisableAndVoidCube = false;

    public override void Initialize() {
        base.Initialize();
        chargeCooldown = GetComponent<ChargeCooldown>();
    }

    protected override bool UsePouvoir() {
        bool returnValue = base.UsePouvoir();
        ennemisAlreadyHitten.Clear();
        hasAlreadyGainChargeForBrisableAndVoidCube = false;
        player.SetPowerDashingFor(dureePowerDahsing);
        return returnValue;
    }

    public bool HitEnnemy(Ennemi ennemi) {
        if(CanHitEnnemy(ennemi)) {
            GainCharge();
            ennemisAlreadyHitten.Add(ennemi);
            gm.soundManager.PlayPowerDashImpactClip();
            player.ResetGrip();
            ennemi.HitByPlayerPowerDash(this);
            StartImpactVfx(ennemi.transform.position);
            ApplyTimeMultiplier(useBrisableTime: false);
            gm.player.onPowerDashImpact.Invoke(this);
            gm.player.onPowerDashEnnemiImpact.Invoke(this, ennemi);
            return true;
        }
        return false;
    }

    public void HitBrisableCube(BrisableCube brisableCube) {
        if (!hasAlreadyGainChargeForBrisableAndVoidCube) {
            GainCharge();
            ApplyTimeMultiplier(useBrisableTime: true);
            gm.player.onPowerDashImpact.Invoke(this);
            hasAlreadyGainChargeForBrisableAndVoidCube = true;
        }
        player.ResetGrip();
        gm.soundManager.PlayPowerDashImpactClip();
        StartImpactVfx(brisableCube.transform.position, dontFakePosition: true);
    }

    public void HitVoidCube(VoidCube voidCube) {
        if (!hasAlreadyGainChargeForBrisableAndVoidCube) {
            GainCharge();
            ApplyTimeMultiplier(useBrisableTime: false);
            gm.player.onPowerDashImpact.Invoke(this);
            hasAlreadyGainChargeForBrisableAndVoidCube = true;
        }
        if (!voidCube.HasVoidExploded()) { // We don't want to do that on other VoidCube than the first one !
            player.ResetGrip();
            gm.soundManager.PlayPowerDashImpactClip();
            StartImpactVfx(voidCube.transform.position, dontFakePosition: true);
        }
    }

    protected void StartImpactVfx(Vector3 position, bool dontFakePosition = false) {
        Vector3 fakeImpactPosition = position;
        if (!dontFakePosition) {
            fakeImpactPosition = position + GetCurrentPoussee().direction * GetCurrentPoussee().distance * coefFakeImpactPosition;
        }
        VisualEffect vfx = Instantiate(impactVfxPrefab, fakeImpactPosition, Quaternion.identity, parent: gm.map.particlesFolder).GetComponent<VisualEffect>();
        vfx.SendEvent("Explode");
        Destroy(vfx.gameObject, vfx.GetVector2("ExplosionLifetime").y);
    }

    protected void GainCharge() {
        if(chargeCooldown != null) {
            chargeCooldown.GainChargeOverMax();
        }
    }

    public bool CanHitEnnemy(Ennemi ennemi) {
        return !ennemisAlreadyHitten.Contains(ennemi);
    }

    protected void ApplyTimeMultiplier(bool useBrisableTime) {
        if(currentTimeMultiplier != null) {
            gm.timerManager.timeMultiplierController.RemoveMultiplier(currentTimeMultiplier);
        }
        TimeMultiplier timeMultiplier = new TimeMultiplier(useBrisableTime ? impactBrisableTimeMultiplier : impactTimeMultiplier);
        currentTimeMultiplier = gm.timerManager.AddTimeMultiplier(timeMultiplier);
    }

    protected override void StartVfx() {
        gm.postProcessManager.StartPowerDashVfx(duree);
    }
}
