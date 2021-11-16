using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierNotIn : GetHelperModifier {

    public bool useCubePositions = true;
    [ConditionalHide("useCubePositions")]
    public GetCubesHelper getCubesHelper;
    [ConditionalHide("!useCubePositions")]
    public GetEmptyPositionsHelper getEmptyPositionsHelper;

    protected List<Vector3> precomputedNotInPositions;

    public override bool IsInArea(Vector3 position) {
        List<Vector3> notInPositions = GetNotInPositions();
        return !notInPositions.Contains(position);
    }

    protected List<Vector3> GetNotInPositions() {
        if(precomputedNotInPositions == null) {
            if(useCubePositions) {
                precomputedNotInPositions = getCubesHelper.Get().Select(c => c.transform.position).ToList();
            } else {
                precomputedNotInPositions = getEmptyPositionsHelper.Get();
            }
        }
        return precomputedNotInPositions;
    }
}
