using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDash : IPouvoir {

    public float distance = 3.0f;
    public float duree = 0.15f;
    public float removeGravityTresholdAngle = 15.0f;

    protected override bool UsePouvoir() {
        Vector3 direction = player.camera.transform.forward;
        Poussee poussee = new Poussee(direction, duree, distance);
        player.AddPoussee(poussee);
        player.ResetGrip();
        RemoveGravityEffect(direction);
        gm.postProcessManager.StartDashVfx(duree);
        return true;
    }

    protected void RemoveGravityEffect(Vector3 direction) {
        float angle = Mathf.Abs(Vector3.Angle(direction, gm.gravityManager.Down()));
        if (90 - angle <= removeGravityTresholdAngle) {
            player.RemoveGravityEffectFor(duree);
        }
    }
}
