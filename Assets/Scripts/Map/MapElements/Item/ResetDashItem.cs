using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ResetDashItem : Item {

    public override void OnTrigger(Collider hit) {
        gm.player.ResetGrip();
        gm.player.ResetAdditionnalJumps();
        gm.player.GetDash().GetCooldown().RechargeEntirely();
        gm.player.StartUncancellableJump(gm.player.GetEtat());
        gm.player.RemoveAllPoussees();
    }
}
