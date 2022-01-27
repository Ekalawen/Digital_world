using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class LinkyGenerator : IGenerator {

    [Header("Linky Generator")]
    public bool setLinky = true;

    public override void Initialize() {
        base.Initialize();
        Assert.IsTrue(choseType != ChoseType.GET_EMPTY);
    }

    protected override void GenerateOneSpecific(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        if(choseType == ChoseType.GET_CUBES) {
            if(cube != null) {
                cube.SetLinkyValue(setLinky);
            }
        }
    }

    protected override bool IsValidPosition(Vector3 position) {
        Cube cube = map.GetCubeAt(position);
        return (choseType == ChoseType.GET_CUBES && cube != null && cube.IsLinky() != setLinky);
    }
}
