using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirTimeHack : IPouvoir {

    public float duree = 5.0f;
    public float rayon = 6.0f;
    public float durationToRecharge = 3.0f;
    public TimeMultiplier timeMultiplier;
    public SpeedMultiplier speedMultiplier;
    public GameObject orbTriggerPrefab;

    protected bool isActive = false;
    protected NoAutomaticRechargeCooldown customCooldown;
    protected OrbTrigger orbTrigger = null;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected SpeedMultiplier currentSpeedMultiplier = null;
    protected float oldGravityIntensity = 0.0f;

    public override void Initialize() {
        base.Initialize();
        customCooldown = GetComponent<NoAutomaticRechargeCooldown>();
    }

    protected override bool UsePouvoir() {
        if (!isActive) {
            StartPouvoir();
        } else {
            StopPouvoir();
        }

        return true;
    }

    protected void StartPouvoir() {
        isActive = true;
        ApplySlowMotion();
        IncreaseSpeed();
        CreateOrbTrigger();
        RemoveGravityIntensity();
        SetInvincible();
        player.RemoveAllPoussees();
        StartVfx();
    }

    protected void RemoveGravityIntensity() {
        oldGravityIntensity = gm.gravityManager.gravityIntensity;
        gm.gravityManager.SetGravity(gm.gravityManager.gravityDirection, 0.0f);
    }

    protected void RestoreGravityIntensity() {
        gm.gravityManager.SetGravity(gm.gravityManager.gravityDirection, oldGravityIntensity);
    }

    protected void CreateOrbTrigger() {
        orbTrigger = Instantiate(orbTriggerPrefab, position: player.transform.position, rotation: Quaternion.identity, parent: gm.map.zonesFolder).GetComponent<OrbTrigger>();
        orbTrigger.Initialize(rayon, duree);
        orbTrigger.onExit.AddListener(StopPouvoir);
        orbTrigger.onHack.AddListener(StopPouvoir);
    }

    protected void StopPouvoir(OrbTrigger orbTrigger) {
        StopPouvoir();
    }

    protected void StopPouvoir() {
        RemoveSlowmotion();
        DecreaseSpeed();
        DestroyOrbTrigger();
        RestoreGravityIntensity();
        UnsetInvincible();
        UseCharge();
        isActive = false;
    }

    protected void UseCharge() {
        customCooldown.Use();
        customCooldown.GainChargeIn(durationToRecharge);
    }

    protected void DestroyOrbTrigger() {
        if (orbTrigger != null) {
            orbTrigger.ReduceAndDestroy();
            orbTrigger = null;
        }
    }

    protected void ApplySlowMotion() {
        gm.timerManager.timeMultiplierController.RemoveAllMultipliers();
        currentTimeMultiplier = gm.timerManager.AddTimeMultiplier(new TimeMultiplier(timeMultiplier));
    }

    protected void RemoveSlowmotion() {
        if(currentTimeMultiplier != null) {
            gm.timerManager.RemoveTimeMultiplier(currentTimeMultiplier);
            currentTimeMultiplier = null;
        }
    }

    protected virtual void StartVfx() {
        // TODO !
        //gm.postProcessManager.StartDashVfx(duree);
    }

    protected void SetInvincible() {
        gm.player.SetInvincible();
    }

    protected void UnsetInvincible() {
        gm.player.UnsetInvincible();
    }

    protected void IncreaseSpeed() {
        gm.player.speedMultiplierController.RemoveAllMultiplier();
        currentSpeedMultiplier = gm.player.AddMultiplier(speedMultiplier);
    }

    protected void DecreaseSpeed() {
        if(currentSpeedMultiplier != null) {
            gm.player.speedMultiplierController.RemoveMultiplier(currentSpeedMultiplier);
            currentSpeedMultiplier = null;
        }
    }
}
