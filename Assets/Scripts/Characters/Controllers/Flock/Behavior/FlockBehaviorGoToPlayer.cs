using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviorGoToPlayer : FlockBehavior {

    public float range = 5.0f;
    public float interpolateFactor = 5;

    protected Player player;

    public override void Initialize() {
        base.Initialize();
        player = gm.player;
    }

    public override Vector3 CalculateMove(IController flockController) {
        if (Vector3.Distance(player.transform.position, flockController.transform.position) <= range) {
            Vector3 direction = (player.transform.position - flockController.transform.position).normalized;
            return (direction + flockController.transform.forward * interpolateFactor) / (1 + interpolateFactor);
        } else {
            return Vector3.zero;
        }
    }
}
