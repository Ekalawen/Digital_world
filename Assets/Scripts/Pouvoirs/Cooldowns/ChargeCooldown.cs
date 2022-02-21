using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class ChargeCooldown : Cooldown {

    public int maxCharges = 1;
    public bool dontStartFull = false;
    [ConditionalHide("dontStartFull")]
    public int startCharges = 0;

    protected int currentCharges;
    protected Coroutine chargingCoroutine = null;

    public override void Initialize() {
        base.Initialize();
        currentCharges = dontStartFull ? startCharges : maxCharges;
    }

    public override void RechargeEntirely() {
        base.RechargeEntirely();
        GainMultipleCharges(maxCharges - currentCharges);
    }

    public override void Use() {
        if(currentCharges <= 0) {
            Debug.LogError($"Tentative d'utiliser une ChargeCooldown sans charges sur le pouvoir {pouvoir.name} !", pouvoir); 
        }
        currentCharges--;
        StartCharging();
    }

    protected virtual void StartCharging() {
        if(chargingCoroutine == null) {
            chargingCoroutine = StartCoroutine(CStartChargingCoroutine());
        }
    }

    protected IEnumerator CStartChargingCoroutine() {
        cooldownTimer.Reset();
        yield return new WaitForSeconds(cooldown);
        chargingCoroutine = null;
        if(currentCharges < maxCharges) {
            currentCharges++;
            pouvoir.GetPouvoirDisplay().FlashPouvoirAvailable();
            if(currentCharges < maxCharges) {
                StartCharging();
            }
        }
    }

    public void GainCharge() {
        GainMultipleCharges(1);
    }

    public void GainMultipleCharges(int nbChargesToGain) {
        if(currentCharges < maxCharges) {
            currentCharges = Mathf.Min(currentCharges + nbChargesToGain, maxCharges);
            pouvoir.GetPouvoirDisplay().FlashPouvoirAvailable();
        }
    }

    public void GainChargeOverMax() {
        GainMultipleChargeOverMax(1);
    }

    public void GainMultipleChargeOverMax(int nbChargesToGain) {
        currentCharges += nbChargesToGain;
        pouvoir.GetPouvoirDisplay().FlashPouvoirAvailable();
    }

    public override bool IsAvailable() {
        return currentCharges > 0;
    }

    public override bool IsCharging() {
        return currentCharges < maxCharges;
    }

    public override float GetTextToDisplayOnPouvoirDisplay() {
        if(!pouvoir.IsEnabled()) {
            return 0.0f;
        }
        if(IsAvailable()) {
            return currentCharges;
        }
        return base.GetTextToDisplayOnPouvoirDisplay();
    }

    public override bool IsTextToDisplayOnPouvoirDisplayATimer() {
        if(!pouvoir.IsEnabled()) {
            return true;
        }
        if(IsAvailable()) {
            return false;
        }
        return base.IsTextToDisplayOnPouvoirDisplayATimer();
    }

    public override bool ShouldDisplayTextOnPouvoirDisplay() {
        return true;
    }

    public int GetCurrentCharges() {
        return currentCharges;
    }

    public override void SetCooldownDuration(float duration, bool keepRemainingTime) {
        base.SetCooldownDuration(duration, keepRemainingTime);
        if(duration == 0.0f) {
            currentCharges = maxCharges;
            if (chargingCoroutine != null) {
                StopCoroutine(chargingCoroutine);
                chargingCoroutine = null;
            }
        }
    }
}
