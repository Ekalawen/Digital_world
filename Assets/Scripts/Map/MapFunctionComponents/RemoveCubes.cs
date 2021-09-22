using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCubes : MapFunctionComponent {

    public GetCubesHelper getCubesHelper;

    public override void Activate() {
        List<Cube> cubes = getCubesHelper.Get();
        foreach(Cube cube in cubes) {
            cube.Destroy();
        }
    }
}
