using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrisableCube : NonBlackCube {

    public float dureeBeforeDestruction = 1.5f;

    protected Coroutine coroutine = null;

    public override void Initialize() {
        base.Initialize();
        SetOpacity(1);
    }

    public override void InteractWithPlayer() {
        if(gm.player.IsPowerDashing()) {
            gm.player.GetPowerDash().HitBrisableCube(this);
            Explode();
        } else if (!IsDecomposing() || GetDureeDecomposeRemaining() > dureeBeforeDestruction) {
            Decompose(dureeBeforeDestruction);
        }
    }
}
