using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviorGoToPlayer : FlockBehavior {

    protected Player player;

    public override void Initialize() {
        base.Initialize();
        player = gm.player;
    }

    public override Vector3 GetMove(IController flockController) {
        return (player.transform.position - flockController.transform.position).normalized;
    }
}
