using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToDirectionController : IController {

    public Vector3 direction;
    public float distanceBeforeDestruction = 1000.0f;

    protected override void UpdateSpecific() {
        Vector3 pointToGo = transform.position + direction.normalized * vitesse * 2;
        Move(pointToGo);

        // Si on est trop loin du jeu on s'auto-détruit ! :)
        if(Vector3.Distance(transform.position, gm.map.GetCenter()) >= distanceBeforeDestruction) {
            Destroy(gameObject);
        }
    }

    public override bool IsInactive() {
        return false;
    }

    public override bool IsMoving() {
        return true;
    }
}
