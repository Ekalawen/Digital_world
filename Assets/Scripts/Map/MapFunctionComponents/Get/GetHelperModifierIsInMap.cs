using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierIsInMap : GetHelperModifier {

    public Vector3 minMapOffsets = Vector3.one * -1;
    public Vector3 maxMapOffset = Vector3.one * -1;

    public override bool IsInArea(Vector3 position) {
        MapManager map = GameManager.Instance.map;
        return IsIn(position.x, map.tailleMap.x, minMapOffsets.x, maxMapOffset.x)
            && IsIn(position.y, map.tailleMap.y, minMapOffsets.y, maxMapOffset.y)
            && IsIn(position.z, map.tailleMap.z, minMapOffsets.z, maxMapOffset.z);
    }

    protected bool IsIn(float value, float tailleMap, float min, float max) {
        return (min == -1 || value >= min)
            && (max == -1 || value <= tailleMap - max);
    }
}
