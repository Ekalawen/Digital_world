using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceHalfByCubeType : GenerateCubesMapFunction {

    public GetCubesHelper getCubesHelper;
    public int moduloParam = 2;
    public int agglomerationParam = 1;

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
        posInt.x /= agglomerationParam;
        posInt.y /= agglomerationParam;
        posInt.z /= agglomerationParam;
        return (posInt.x + posInt.y + posInt.z) % moduloParam == 0;
    }
}
