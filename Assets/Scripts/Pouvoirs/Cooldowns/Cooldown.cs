using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;

public class Cooldown : MonoBehaviour {

    public float cooldown = 1.0f;

    protected Timer cooldownTimer;
    protected IPouvoir pouvoir;

    public virtual void Initialize() {
        pouvoir = GetComponent<IPouvoir>();
        cooldownTimer = new Timer(cooldown, setOver: true);
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
        if(IsAvailable()) {
            return 0.0f;
        }
        return cooldownTimer.GetRemainingTime();
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
}
