using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToDirectionController : IController {

    public Vector3 direction;

    protected override void UpdateSpecific() {
        Vector3 pointToGo = transform.position + direction.normalized * vitesse * 2;
        Move(pointToGo);
    }

    public override bool IsInactive() {
        return false;
    }

    public override bool IsMoving() {
        return true;
    }
}
