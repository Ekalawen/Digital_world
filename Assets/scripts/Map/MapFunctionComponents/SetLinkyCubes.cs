using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetLinkyCubes : MapFunctionComponent {

    public GetCubesHelper getCubesHelper;
    public bool linkyValue = true;

    public override void Activate() {
        List<Cube> cubes = getCubesHelper.Get();
        foreach(Cube cube in cubes) {
            cube.SetLinkyValue(linkyValue);
        }
    }
}
