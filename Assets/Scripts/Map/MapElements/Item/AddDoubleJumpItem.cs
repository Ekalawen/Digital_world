using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddDoubleJumpItem : Item {

    public int nbDoubleJumpAdded = 1;

    public override void OnTrigger(Collider hit) {
        gm.player.AddDoubleJump(nbDoubleJumpAdded);
        gm.console.CaptureAddDoubleJump();
    }
}
