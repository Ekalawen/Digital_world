using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDash : IPouvoir {

    public float distance = 3.0f;
    public float duree = 0.15f;
    public float removeGravityTresholdAngle = 15.0f;

    protected Poussee currentPoussee = null;
    protected bool shouldResetGrip = false;
    protected bool shouldRemoveAllNegativePoussees = false;

    public override void Initialize() {
        base.Initialize();
        InitializeDashDistance();
        InitializeDashCharges();
        InitializeShouldResetGrip();
        InitializeShouldRemoveAllNegativePoussees();
    }

    protected void InitializeShouldRemoveAllNegativePoussees() {
        shouldRemoveAllNegativePoussees = SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_CANCEL_POUSSEES);
    }

    protected void InitializeShouldResetGrip() {
        shouldResetGrip = SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_RESET_GRIP);
    }

    protected void InitializeDashCharges() {
        ChargeCooldown chargeCooldown = cooldown as ChargeCooldown;
        if(!chargeCooldown) {
            return;
        }
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_CHARGE_PLUS_1)) {
            chargeCooldown.maxCharges += 1;
        }
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_CHARGE_PLUS_2)) {
            chargeCooldown.maxCharges += 1;
        }
        chargeCooldown.Initialize();
    }

    protected void InitializeDashDistance() {
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_LENGTH_PLUS_1)) {
            distance += 1;
        }
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_LENGTH_PLUS_2)) {
            distance += 1;
        }
        if (SkillTreeManager.Instance.IsEnabled(SkillKey.DASH_LENGTH_PLUS_3)) {
            distance += 1;
        }
    }

    protected override bool UsePouvoir() {
        Vector3 direction = player.camera.transform.forward;
        currentPoussee = new Poussee(direction, duree, distance);
        player.AddPoussee(currentPoussee);
        RemoveAllNegativePoussees();
        ResetGripWhileDashing();
        RemoveGravityEffect(direction);
        StartVfx();
        gm.timerManager.timeMultiplierController.RemoveAllEnnemisMultipliers();
        player.onUseDash.Invoke(this);
        return true;
    }

    private void RemoveAllNegativePoussees() {
        if (shouldRemoveAllNegativePoussees) {
            player.RemoveAllNegativePoussees();
        }
    }

    protected void ResetGripWhileDashing() {
        if (shouldResetGrip) {
            StartCoroutine(CResetGripWhileDashing());
        }
    }

    protected IEnumerator CResetGripWhileDashing() {
        Timer timer = new Timer(duree);
        while (!timer.IsOver()) {
            player.ResetGrip();
            yield return null;
        }
    }

    protected virtual void StartVfx() {
        gm.postProcessManager.StartDashVfx(duree);
    }

    protected void RemoveGravityEffect(Vector3 direction) {
        float angle = Mathf.Abs(Vector3.Angle(direction, gm.gravityManager.Down()));
        if (90 - angle <= removeGravityTresholdAngle) {
            player.RemoveGravityEffectFor(duree);
        }
    }

    public Poussee GetCurrentPoussee() {
        return currentPoussee;
    }
}
