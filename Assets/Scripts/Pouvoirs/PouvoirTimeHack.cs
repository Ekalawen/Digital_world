using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PouvoirTimeHack : IPouvoir {

    [Header("Time")]
    public float slowmotionFactor = 20;
    public float duree = 5.0f;
    public TimeMultiplier timeMultiplier;
    public SpeedMultiplier speedMultiplier;

    [Header("OrbTrigger")]
    public float rayon = 6.0f;
    public float startingRayon = 2.0f;
    public GameObject orbTriggerPrefab;

    [Header("Cooldown")]
    public float durationToRecharge = 3.0f;

    protected bool isActive = false;
    protected NoAutomaticRechargeCooldown customCooldown;
    protected OrbTrigger orbTrigger = null;
    protected TimeMultiplier currentTimeMultiplier = null;
    protected SpeedMultiplier currentSpeedMultiplier = null;

    public override void Initialize() {
        base.Initialize();
        InitializeSlowmotionFactor();
        customCooldown = GetComponent<NoAutomaticRechargeCooldown>();
    }

    protected void InitializeSlowmotionFactor() {
        timeMultiplier.timeVariation = 1.0f / slowmotionFactor;
        speedMultiplier.speedAdded = slowmotionFactor - 1.0f;
    }

    protected override void ApplyUsePouvoir() {
        if (!isActive) {
            UsePouvoir();
            //ApplyUsePouvoirConsequences();
            player.onUsePouvoir.Invoke(this);
        } else {
            StopPouvoir();
        }
    }

    protected override bool UsePouvoir() {
        if(isActive) {
            return false;
        }
        isActive = true;
        ApplySlowMotion();
        IncreaseSpeed();
        CreateOrbTrigger();
        RemoveGravityIntensity();
        UnrobbIfRobbed();
        SetInvincible();
        RechargeAllOtherPouvoirs();
        player.RemoveAllPoussees();
        player.onTimeHackStart.Invoke(this);
        StartVfx();
        return true;
    }

    protected void RemoveGravityIntensity() {
        gm.gravityManager.SetIntensity(0.0f);
    }

    protected void RestoreGravityIntensity() {
        gm.gravityManager.SetIntensity(gm.gravityManager.initialGravityIntensity);
    }

    protected void CreateOrbTrigger() {
        orbTrigger = Instantiate(orbTriggerPrefab, position: player.transform.position, rotation: Quaternion.identity, parent: gm.map.zonesFolder).GetComponent<OrbTrigger>();
        orbTrigger.Initialize(rayon, duree);
        orbTrigger.Resize(orbTrigger.transform.position, Vector3.one * startingRayon);
        orbTrigger.ResizeOverTime(rayon, orbTrigger.dureeConstruction);
        orbTrigger.onExit.AddListener(StopPouvoir);
        orbTrigger.onHack.AddListener(StopPouvoir);
    }

    protected void StopPouvoir(OrbTrigger orbTrigger) {
        StopPouvoir();
    }

    protected void StopPouvoir() {
        if(!isActive) {
            return;
        }
        isActive = false;
        RemoveSlowmotion();
        DecreaseSpeed();
        DestroyOrbTrigger();
        RestoreGravityIntensity();
        UnsetInvincible();
        UseCharge();
        player.ResetGrip();
        player.onTimeHackStop.Invoke(this);
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

    public bool IsActive() {
        return isActive;
    }

    public override bool IsEnabled() {
        return pouvoirEnabled && !pouvoirFreezed; // We want to be able to press E a second time to stop TimeHack :)
    }

    protected void RechargeAllOtherPouvoirs() {
        player.GetPathfinder()?.GetCooldown().RechargeEntirely();
        player.GetDash()?.GetCooldown().RechargeEntirely();
        player.GetGripDash()?.GetCooldown().RechargeEntirely();
    }

    protected void UnrobbIfRobbed() {
        if(SoulRobber.IsPlayerRobbed()) {
            SoulRobber oneSoulRobber = gm.ennemiManager.GetEnnemisOfType<SoulRobber>().First();
            if(oneSoulRobber != null) {
                oneSoulRobber.StartUnrobb();
            }
        }
    }
}
