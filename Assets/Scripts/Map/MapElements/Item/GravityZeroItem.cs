using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityZeroItem : Item {

    public override void OnTrigger(Collider hit) {
        gm.gravityManager.SetGravity(gm.gravityManager.gravityDirection, 0.0f);
        gm.console.CapturePouvoirGiverVoler();
    }

}
