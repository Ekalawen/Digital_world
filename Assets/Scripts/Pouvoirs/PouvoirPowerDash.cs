using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    public GameObject lightningPrefab;
    public TimeMultiplier impactTimeMultiplier;

    protected List<Ennemi> ennemisAlreadyHitten = new List<Ennemi>();
    protected ChargeCooldown chargeCooldown;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected bool hasAlreadyGainChargeForBrisableCube = false;

    public override void Initialize() {
        base.Initialize();
        chargeCooldown = GetComponent<ChargeCooldown>();
    }

    protected override bool UsePouvoir() {
        bool returnValue = base.UsePouvoir();
        ennemisAlreadyHitten.Clear();
        hasAlreadyGainChargeForBrisableCube = false;
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
            ApplyTimeMultiplier();
            return true;
        }
        return false;
    }

    public void HitBrisableCube(Cube cube) {
        if (!hasAlreadyGainChargeForBrisableCube) {
            GainCharge();
            hasAlreadyGainChargeForBrisableCube = true;
        }
        player.ResetGrip();
        gm.soundManager.PlayPowerDashImpactClip();
        StartImpactVfx(cube.transform.position, dontFakePosition: true);
        ApplyTimeMultiplier();
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

    protected void ApplyTimeMultiplier() {
        if(currentTimeMultiplier != null) {
            gm.timerManager.timeMultiplierController.RemoveMultiplier(currentTimeMultiplier);
        }
        currentTimeMultiplier = gm.timerManager.AddTimeMultiplier(new TimeMultiplier(impactTimeMultiplier));
    }

    protected override void StartVfx() {
        gm.postProcessManager.StartPowerDashVfx(duree);
    }
}
