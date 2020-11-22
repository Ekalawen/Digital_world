using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceHalfByCubeType : GenerateCubesMapFunction {

    public GetCubesHelper getCubesHelper;

    public override void Activate() {
        List<Cube> cubes = getCubesHelper.Get();
        foreach(Cube cube in cubes) {
            if(ShouldBeReplaced(cube)) {
                map.SwapCubeType(cube, cubeType);
            }
        }
    }

    protected bool ShouldBeReplaced(Cube cube) {
        Vector3Int posInt = MathTools.RoundToInt(cube.transform.position);
        return (posInt.x + posInt.y + posInt.z) % 2 == 0;
    }
}
