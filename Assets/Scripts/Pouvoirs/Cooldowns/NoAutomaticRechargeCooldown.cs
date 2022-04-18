using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class NoAutomaticRechargeCooldown : ChargeCooldown {

    protected List<Timer> rechargers;

    public override void Initialize() {
        base.Initialize();
        rechargers = new List<Timer>();
    }

    protected override void StartCharging() {
        // Don't recharge here !
    }

    public override bool IsAvailable() {
        return currentCharges > 0;
    }

    public override bool IsCharging() {
        return rechargers.Count > 0;
    }

    public override void GainChargeIn(float duration) {
        StartCoroutine(CGainChargeInWithRecharger(duration));
    }

    protected IEnumerator CGainChargeInWithRecharger(float duration) {
        Timer recharger = new Timer(duration);
        rechargers.Add(recharger);
        yield return new WaitForSeconds(duration);
        GainCharge();
        rechargers.Remove(recharger);
    }

    public override float GetTextToDisplayOnPouvoirDisplay() {
        if(!pouvoir.IsEnabled()) {
            return 0.0f;
        }
        if(IsAvailable()) {
            return currentCharges;
        }
        if(rechargers.Count == 0) {
            return 0f;
        }
        return rechargers.Min(r => r.GetRemainingTime());
    }

    public override bool IsTextToDisplayOnPouvoirDisplayATimer() {
        if (!pouvoir.IsEnabled()) {
            return true;
        }
        if (IsAvailable()) {
            return false;
        }
        if(rechargers.Count == 0) {
            return false;
        }
        return base.IsTextToDisplayOnPouvoirDisplayATimer();
    }
}
