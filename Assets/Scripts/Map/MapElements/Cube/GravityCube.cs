using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class GravityCube : NonBlackCube {

    public override void Initialize() {
        base.Initialize();
        if(gm.player.DoubleCheckInteractWithCube(this)) {
            InteractWithPlayer();
        }
    }

    public override void InteractWithPlayer() {
        Player player = gm.player;
        Vector3 directionVector = GetGravityDirection(player.transform.position);
        GravityManager.Direction direction = GravityManager.OppositeDir(GravityManager.VecToDir(directionVector));
        if (direction != gm.gravityManager.gravityDirection) {
            gm.gravityManager.SetGravity(direction, gm.gravityManager.gravityIntensity);
            gm.player.ResetGrip();
        }
    }

    protected Vector3 GetGravityDirection(Vector3 position) {
        Vector3 direction = (position - transform.position).normalized;
        Vector3 directionPoussee = MathTools.GetClosestToNormals(transform, direction);
        return directionPoussee;
    }
}
