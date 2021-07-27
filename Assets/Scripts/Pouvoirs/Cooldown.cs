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

    public virtual float GetRemainingTimeBeforeUse() {
        if(IsAvailable()) {
            return 0.0f;
        }
        return cooldownTimer.GetRemainingTime();
    }

    public void SetCooldownDuration(float duration, bool keepRemainingTime) {
        float remainingTime = GetRemainingTimeBeforeUse();
        cooldownTimer = new Timer(duration);
        if(keepRemainingTime) {
            cooldownTimer.SetRemainingTime(remainingTime);
        }
    }

}
