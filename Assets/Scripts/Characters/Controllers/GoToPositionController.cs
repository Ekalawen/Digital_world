using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToPositionController : IController {

    protected Vector3 target;

    public override void Start() {
        base.Start();
        target = transform.position;
    }

    protected override void UpdateSpecific() {
        MoveToTarget(target);
    }

    public override bool IsInactive() {
        return MathTools.AlmostEqual(transform.position, target);
    }

    public override bool IsMoving() {
        return !IsInactive();
    }

    public void GoTo(Vector3 target) {
        this.target = target;
    }
}
