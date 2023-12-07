using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Localization;

public class Cooldown : MonoBehaviour {

    public float cooldown = 1.0f;

    protected Timer cooldownTimer;
    protected IPouvoir pouvoir;
    protected List<CooldownModifier> modifiers;

    public virtual void Initialize() {
        pouvoir = GetComponent<IPouvoir>();
        cooldownTimer = new Timer(cooldown, setOver: true);
        modifiers = GetComponents<CooldownModifier>().ToList();
        modifiers.ForEach(modifier => modifier.Initialize(this));
    }

    public virtual void RechargeEntirely() {
        cooldownTimer.SetOver();
    }

    public virtual void Use() {
        cooldownTimer.Reset();
    }

    public virtual bool IsAvailable() {
        return cooldownTimer.IsOver();
    }

    public virtual bool IsCharging() {
        return !IsAvailable();
    }

    public virtual float GetRemainingTimeBeforeUse() {
        return IsAvailable() ? 0.0f : cooldownTimer.GetRemainingTime();
    }

    public virtual void SetCooldownDuration(float duration, bool keepRemainingTime) {
        cooldown = duration;
        cooldownTimer = new Timer(duration);
        if(keepRemainingTime) {
            float remainingTime = GetRemainingTimeBeforeUse();
            cooldownTimer.SetRemainingTime(remainingTime);
        }
    }

    public virtual float GetTextToDisplayOnPouvoirDisplay() {
        if(!pouvoir.IsEnabled()) {
            return 0.0f;
        }
        return cooldownTimer.GetRemainingTime();
    }

    public virtual bool IsTextToDisplayOnPouvoirDisplayATimer() {
        return true;
    }

    public virtual bool ShouldDisplayTextOnPouvoirDisplay() {
        return !IsAvailable() || !pouvoir.IsEnabled();
    }

    public virtual void GainCharge() {
        cooldownTimer.SetOver();
    }

    public virtual void GainChargeIn(float duration) {
        StartCoroutine(CGainChargeIn(duration));
    }

    protected IEnumerator CGainChargeIn(float duration) {
        yield return new WaitForSeconds(duration);
        GainCharge();
    }

    public IPouvoir GetPouvoir() {
        return pouvoir;
    }
}
