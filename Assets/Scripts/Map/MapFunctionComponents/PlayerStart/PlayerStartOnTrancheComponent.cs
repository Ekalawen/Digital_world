using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartOnTrancheComponent : PlayerStartComponent {

    public GravityManager.Direction fromDirection = GravityManager.Direction.BAS;
    public int offset = 1;

    public override Vector3 GetPlayerStartPosition() {
        List<Vector3> positions = map.GetAllEmptyPositions().FindAll(p => IsInTranche(p, fromDirection, offset));
        if(positions.Count == 0) {
            return base.GetPlayerStartPosition();
        } else {
            return positions[UnityEngine.Random.Range(0, positions.Count)];
        }
    }

    public bool IsInTranche(Vector3 pos, GravityManager.Direction fromDirection, int offset) {
        Vector3 directionVecteur = GravityManager.DirToVec(fromDirection);
        if(fromDirection == GravityManager.Direction.BAS || fromDirection == GravityManager.Direction.DROITE || fromDirection == GravityManager.Direction.AVANT) {
            float trancheIndice = Vector3.Dot(pos, -directionVecteur);
            return trancheIndice == offset;
        } else {
            int tailleMap = map.GetTailleMapAlongDirection(fromDirection);
            float trancheIndice = tailleMap - Vector3.Dot(pos, directionVecteur);
            return trancheIndice == offset;
        }
    }
}
