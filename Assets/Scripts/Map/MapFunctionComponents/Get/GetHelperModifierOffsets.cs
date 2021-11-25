using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierOffsets : GetHelperModifier {

    public Vector3 minOffsets = Vector3.one * -1;
    public Vector3 maxOffsets = Vector3.one * -1;

    public override bool IsInArea(Vector3 position) {
        MapManager map = GameManager.Instance.map;
        return IsIn(position.x, map.tailleMap.x, minOffsets.x, maxOffsets.x)
            && IsIn(position.y, map.tailleMap.y, minOffsets.y, maxOffsets.y)
            && IsIn(position.z, map.tailleMap.z, minOffsets.z, maxOffsets.z);
    }

    protected bool IsIn(float value, float tailleMap, float min, float max) {
        return (min == -1 || value >= min)
            && (max == -1 || value <= tailleMap - max);
    }
}
