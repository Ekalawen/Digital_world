using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDash : IPouvoir {

    public float distance = 3.0f;
    public float duree = 0.15f;
    public float removeGravityTresholdAngle = 15.0f;

    protected Poussee currentPoussee = null;

    protected override bool UsePouvoir()
    {
        Vector3 direction = player.camera.transform.forward;
        currentPoussee = new Poussee(direction, duree, distance);
        player.AddPoussee(currentPoussee);
        player.RemoveAllNegativePoussees();
        ResetGripWhileDashing();
        RemoveGravityEffect(direction);
        StartVfx();
        gm.timerManager.timeMultiplierController.RemoveAllEnnemisMultipliers();
        player.onUseDash.Invoke(this);
        return true;
    }

    protected void ResetGripWhileDashing() {
        StartCoroutine(CResetGripWhileDashing());
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
