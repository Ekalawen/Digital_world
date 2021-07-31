using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierEmptyBoxes : GetHelperModifier {

    public int modulo = 3;
    public Vector2Int nbAxesToCheck = new Vector2Int(2, 3);

    public override bool IsInArea(Vector3 position) {
        Vector3Int posInt = MathTools.RoundToInt(position);
        int nbAxesChecked = posInt.x % modulo == 0 ? 1 : 0;
        nbAxesChecked += posInt.y % modulo == 0 ? 1 : 0;
        nbAxesChecked += posInt.z % modulo == 0 ? 1 : 0;
        return nbAxesToCheck[0] <= nbAxesChecked && nbAxesChecked <= nbAxesToCheck[1];
    }
}
