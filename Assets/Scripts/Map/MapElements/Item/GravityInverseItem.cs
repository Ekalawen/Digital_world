using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityInverseItem : Item {

    public override void OnTrigger(Collider hit) {
        GravityManager.Direction up = GravityManager.OppositeDir(gm.gravityManager.gravityDirection);
        gm.gravityManager.SetGravity(up, gm.gravityManager.gravityIntensity);
    }

}
