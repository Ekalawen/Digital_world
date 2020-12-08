using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushPlayerController : EnnemiController {
    protected override void UpdateSpecific() {
        MoveToTarget(player.transform.position);
    }

    public override bool IsInactive() {
        return false;
    }

    public override bool IsMoving() {
        return true;
    }
}
