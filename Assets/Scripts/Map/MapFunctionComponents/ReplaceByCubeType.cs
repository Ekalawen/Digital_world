using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReplaceByCubeType : GenerateCubesMapFunction {

    public GetCubesHelper getCubesHelper;
    public bool setLinky = false;

    public override void Activate() {
        List<Cube> cubes = getCubesHelper.Get();
        foreach(Cube cube in cubes) {
            Cube newCube = map.SwapCubeType(cube, cubeType);
            if(newCube != null && setLinky) {
                newCube.SetLinkyValue(true);
            }
        }
    }
}
