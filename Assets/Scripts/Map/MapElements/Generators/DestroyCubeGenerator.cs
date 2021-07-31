using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DestroyCubeGenerator : IGenerator {


    [Header("DestroyCubeGenerator")]
    public float decomposeTime = 0.5f;

    protected override void GenerateOneSpecific(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        if(cube != null) {
            cube.Decompose(decomposeTime);
        }
    }

    protected override bool IsValidPosition(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        return cube != null;
    }
}
