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
        Vector3 directionVector = GetGravityDirection(gm.player.transform.position);
        GravityManager.Direction direction = GravityManager.OppositeDir(GravityManager.VecToDir(directionVector));
        if (CanChangeGravity(direction, directionVector)) {
            gm.gravityManager.SetGravity(direction, gm.gravityManager.gravityIntensity);
            gm.player.SetAuSol(useSound: false);
        }
    }

    protected bool CanChangeGravity(GravityManager.Direction direction, Vector3 directionVector) {
        Vector3 contactPoint = MathTools.AABBPoint_ContactPoint(transform.position, VectorHalfExtent(), gm.player.transform.position);
        return direction != gm.gravityManager.gravityDirection && !IsInEdges(contactPoint);
    }

    protected Vector3 GetGravityDirection(Vector3 position) {
        Vector3 direction = (position - transform.position).normalized;
        Vector3 gravityDirection = MathTools.GetClosestToNormals(transform, direction);
        gravityDirection = MathTools.SanitizeIfOrthogonal(gravityDirection);
        return gravityDirection;
    }
}
