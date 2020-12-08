using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlirdController : EnnemiController {

    protected FlockManager flockManager;

    public override void Start() {
        base.Start();
        flockManager = gm.flockManager;
        flockManager.Register(this);
        transform.rotation = UnityEngine.Random.rotationUniform;
    }

    protected void OnDestroy() {
        flockManager.Unregister(this);
    }

    protected override void UpdateSpecific() {
        Vector3 move = flockManager.CalculateMove(this);
        MoveWithMove(move);
        transform.LookAt(transform.position + move);
    }

    public override bool IsMoving() {
        return IsPlayerVisible();
    }

    public override bool IsInactive() {
        return !IsMoving();
    }
}
