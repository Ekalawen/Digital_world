using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartOnTrancheComponent : PlayerStartComponent {

    public GravityManager.Direction fromDirection = GravityManager.Direction.BAS;
    public int offset = 1;

    public override Vector3 GetRawPlayerStartPosition() {
        List<Vector3> positions = map.GetAllEmptyPositionsInTranche(fromDirection, offset);
        if(positions.Count == 0) {
            return base.GetPlayerStartPosition();
        } else {
            return positions[UnityEngine.Random.Range(0, positions.Count)];
        }
    }
}
