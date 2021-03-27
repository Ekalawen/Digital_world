using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PouvoirDash : IPouvoir {

    public float distance = 3.0f;
    public float duree = 0.15f;

    protected override bool UsePouvoir() {
        Vector3 direction = player.camera.transform.forward;
        Poussee poussee = new Poussee(direction, duree, distance);
        player.AddPoussee(poussee);
        player.ResetGrip();
        player.RemoveGravityEffectFor(duree);
        gm.postProcessManager.StartDashVfx(duree);
        return true;
    }
}
