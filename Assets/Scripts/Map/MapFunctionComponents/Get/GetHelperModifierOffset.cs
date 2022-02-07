using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GetHelperModifierOffset : GetHelperModifier {

    public Vector3 offset = Vector3.zero;

    public override List<Cube> ModifyCubes(List<Cube> cubes) {
        MapManager map = GameManager.Instance.map;
        List<Vector3> cubePositionsOffseted = ModifyEmpties(cubes.Select(c => c.transform.position).ToList());
        List<Cube> cubesOffseted = cubePositionsOffseted.Select(p => map.GetCubeAt(p)).ToList().FindAll(c => c != null);
        return cubesOffseted;
    }

    public override List<Vector3> ModifyEmpties(List<Vector3> positions) {
        return positions.Select(p => p + offset).ToList();
    }

    public override bool IsInArea(Vector3 position) {
        return true; // Not used !
    }
}
