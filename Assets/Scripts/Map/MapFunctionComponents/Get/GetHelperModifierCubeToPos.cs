using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierCubeToPos : GetHelperModifier {

    public GetCubesHelper getCubes;

    protected List<Vector3> cubesPositions = null;

    public override List<Cube> ModifyCubes(List<Cube> cubes) {
        return getCubes.Get();
    }

    public override List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return getCubes.Get().Select(c => c.transform.position).ToList();
    }

    public override bool IsInArea(Vector3 position) {
        if(cubesPositions == null) {
            cubesPositions = getCubes.Get().Select(c => c.transform.position).ToList();
        }
        return cubesPositions.Contains(position);
    }
}
