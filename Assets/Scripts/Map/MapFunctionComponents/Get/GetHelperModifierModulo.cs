using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierModulo : GetHelperModifier {

    public int modulo = 2;
    public int agglomeration = 1;
    public Vector2Int trueRange = new Vector2Int(0, 0);

    public override List<Cube> ModifyCubes(List<Cube> cubes) {
        return cubes.FindAll(c => IsInArea(c.transform.position));
    }

    public override List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return positions.FindAll(p => IsInArea(p));
    }

    public bool IsInArea(Vector3 position) {
        Vector3Int posInt = MathTools.RoundToInt(position);
        posInt.x /= agglomeration;
        posInt.y /= agglomeration;
        posInt.z /= agglomeration;
        int moduloValue = (posInt.x + posInt.y + posInt.z) % modulo;
        return trueRange[0] <= moduloValue && moduloValue <= trueRange[1];
    }
}
